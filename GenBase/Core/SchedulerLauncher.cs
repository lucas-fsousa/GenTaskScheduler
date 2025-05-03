using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Infra.Log;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core;

internal class SchedulerLauncher(SchedulerConfiguration config, IServiceProvider serviceProvider, ILogger<ApplicationLogger> logger): ISchedulerLauncher {
  private readonly SemaphoreSlim _semaphore = new(config.MaxTasksDegreeOfParallelism);

  private async Task<List<CurrentScheduledTaskInfo>> RefreshData(CancellationToken cancellationToken) {
    logger.LogInformation("Searching for new tasks");
    using var scope = serviceProvider.CreateScope();
    var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    var tasks = await taskRepository.GetAllAsync(cancellationToken);

    var filteredList = tasks
      .Where(t => t.IsActive && t.ExecutionStatus != ExecutionStatus.Running && t.Triggers.Count > 0)
      .Where(t => {
        if(t.DependsOnTask is null)
          return true;

        var parent = tasks.FirstOrDefault(d => d.Id == t.DependsOnTaskId);
        if(parent is null)
          return false;

        return parent.ExecutionStatus == t.DependsOnStatus &&
          parent.ExecutionHistory.Count > 0 &&
          parent.ExecutionHistory.Last().Status == t.DependsOnStatus;
      })
      .Select(t => new CurrentScheduledTaskInfo(t, MatchTrigger(t)))
      .Where(t => t.TriggerId is not null);

    return [.. filteredList];
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken = default) {
    logger.LogInformation("SchedulerLauncher is starting...");
    while(!cancellationToken.IsCancellationRequested) {
      var executableTasks = await RefreshData(cancellationToken);

      foreach(var taskInfo in executableTasks) {
        _ = Task.Run(async () => {
          try {
            await _semaphore.WaitAsync(cancellationToken);
            using(logger.BeginScope("ID={id}", taskInfo.Task.CreatedAt.Ticks.ToString("x2"))) {
              logger.LogInformation("Execution started");
              await ExecuteTaskScopedAsync(taskInfo, cancellationToken);
              logger.LogInformation("Execution completed");
            }
          } finally {
            _semaphore.Release();
          }
        }, cancellationToken);
      }

      await Task.Delay(config.DatabaseCheckInterval, cancellationToken);
    }
  }

  private async Task ExecuteTaskScopedAsync(CurrentScheduledTaskInfo taskInfo, CancellationToken cancellationToken) {
    using var scope = serviceProvider.CreateScope();
    var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    var historyRepository = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();

    await ExecuteWithTrackingAsync(taskInfo, taskRepository, historyRepository, cancellationToken);
  }

  private async Task ExecuteWithTrackingAsync(CurrentScheduledTaskInfo taskInfo, ITaskRepository taskRepository, ITaskHistoryRepository historyRepository, CancellationToken cancellationToken) {
    var history = new TaskExecutionHistory {
      TaskId = taskInfo.Task.Id,
      StartedAt = DateTimeOffset.UtcNow,
      TriggerId = taskInfo.TriggerId
    };

    try {
      if(taskInfo.Task.ExecutionStatus == ExecutionStatus.Running) {
        logger.LogWarning("Task {Id} is already running. Skipping execution.", taskInfo.Task.Id);
        return;
      }

      var baseTrigger = taskInfo.Task.Triggers.First(t => t.Id == taskInfo.TriggerId);
      taskInfo.Task.ExecutionStatus = ExecutionStatus.Running;
      await taskRepository.UpdateAsync(taskInfo.Task, cancellationToken: cancellationToken);
      var retryCount = 1;
      var job = TaskSerializer.Deserialize(taskInfo.Task.BlobArgs);

      while(retryCount <= config.MaxRetry) {
        try {
          logger.LogInformation("Execution attempt {retryCount} of {MaxRetry}", retryCount, config.MaxRetry);
          var result = await job.ExecuteJobAsync(cancellationToken);
          history.EndedAt = DateTimeOffset.UtcNow;
          history.Status = ExecutionStatus.Success;
          history.ResultBlob = TaskSerializer.SerializeObjectToBytes(result);

          taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
          taskInfo.Task.ExecutionStatus = ExecutionStatus.Success;
          logger.LogInformation("Execution attempt {retryCount} status: Success", retryCount);
          break;
        } catch when(config.RetryOnFailure && retryCount < config.MaxRetry - 1) {
          logger.LogWarning("Failed. Waiting {RetryWaitDelay} to try again.", config.RetryWaitDelay);
          retryCount++;
          await Task.Delay(config.RetryWaitDelay, cancellationToken);
        } catch(Exception finalEx) {
          history.EndedAt = DateTimeOffset.UtcNow;
          history.Status = ExecutionStatus.Failed;
          history.ErrorMessage = finalEx.Message;

          taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
          taskInfo.Task.ExecutionStatus = ExecutionStatus.Failed;

          logger.LogError(finalEx, "The execution failed on all attempts.");
          break;
        }
      }

      baseTrigger.LastExecution = DateTimeOffset.UtcNow;
      baseTrigger.Executions++;
      switch(baseTrigger) {
        case OnceTrigger once:
          once.Executed = true;
          break;
        default:
          break;
      }

      if(taskInfo.Task.AutoDelete) {
        logger.LogInformation($"Task configured to auto-delete after execution.");
        await taskRepository.DeleteAsync(taskInfo.Task.Id, cancellationToken: cancellationToken);
        logger.LogInformation("Task deleted successfully");
      } else {
        taskInfo.Task.ExecutionStatus = ExecutionStatus.Success;
        await taskRepository.UpdateAsync(taskInfo.Task, cancellationToken: cancellationToken);
      }
    } catch(Exception ex) {
      history.EndedAt = DateTimeOffset.UtcNow;
      history.Status = ExecutionStatus.Failed;
      history.ErrorMessage = ex.Message;
    } finally {
      if(taskInfo.Task.ExecutionStatus == ExecutionStatus.Running && cancellationToken.IsCancellationRequested)
        taskInfo.Task.ExecutionStatus = ExecutionStatus.Cancelled;
    }

    logger.LogInformation("Saving execution history");
    await historyRepository.AddAsync(history, cancellationToken: cancellationToken);
    await ValidateAndPruneTriggersAsync(taskInfo.Task, taskRepository, cancellationToken);
  }

  private Guid? MatchTrigger(ScheduledTask task) {
    if(!task.IsActive || task.Triggers is null || task.Triggers.Count <= 0)
      return null;

    var utcNow = DateTimeOffset.UtcNow;
    foreach(var trigger in task.Triggers.Where(t => t.IsValid)) {
      return trigger switch {
        CronTrigger cron => TriggerEvaluator.ShouldExecuteCronTrigger(cron, utcNow, config),
        OnceTrigger once => TriggerEvaluator.ShouldExecuteOnceTrigger(once, utcNow, config),
        CalendarTrigger calendar => TriggerEvaluator.ShouldExecuteCalendarTrigger(calendar, utcNow, config),
        IntervalTrigger interval => TriggerEvaluator.ShouldExecuteIntervalTrigger(interval, utcNow, config),
        MonthlyTrigger dwmt => TriggerEvaluator.ShouldExecuteDayWeekMonthTrigger(dwmt, utcNow, config),
        _ => null
      };
    }

    return null;
  }

  private async Task ValidateAndPruneTriggersAsync(ScheduledTask task, ITaskRepository taskRepository, CancellationToken cancellationToken) {
    var utcNow = DateTimeOffset.UtcNow;
    var hasChanges = false;

    if(!task.IsActive || task.Triggers == null || task.Triggers.Count == 0)
      return;

    foreach(var trigger in task.Triggers) {
      switch(trigger) {
        case OnceTrigger once when once.IsValid && (once.Executed || once.StartsAt < utcNow - config.MarginOfError || once.Executions >= once.MaxExecutions):
          once.IsValid = false;
          hasChanges = true;
          break;

        case IntervalTrigger interval when interval.MaxExecutions.HasValue && interval.Executions >= interval.MaxExecutions:
          interval.IsValid = false;
          hasChanges = true;
          break;

        case CronTrigger cron when cron.EndsAt.HasValue && cron.EndsAt.Value < utcNow:
          cron.IsValid = false;
          hasChanges = true;
          break;

        case CalendarTrigger calendar when calendar.EndsAt.HasValue && calendar.EndsAt.Value < utcNow:
          calendar.IsValid = false;
          hasChanges = true;
          break;

        case MonthlyTrigger dwmt when dwmt.EndsAt.HasValue && dwmt.EndsAt.Value < utcNow:
          dwmt.IsValid = false;
          hasChanges = true;
          break;
      }
    }

    if(hasChanges) {
      logger.LogInformation("Invalid or expired trigger identified for taskId {Id}. Applying update and removing from execution list.", task.Id);
      await taskRepository.UpdateAsync(task, cancellationToken: cancellationToken);
    }
  }
}
