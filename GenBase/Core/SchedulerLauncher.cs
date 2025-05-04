using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Models.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core;

internal class SchedulerLauncher(IServiceProvider serviceProvider, ILogger<ApplicationLogger> logger): ISchedulerLauncher {
  private static readonly SchedulerConfiguration _config = GenSchedulerEnvironment.SchedulerConfiguration;
  private static readonly SemaphoreSlim _semaphore = new(_config.MaxTasksDegreeOfParallelism);

  private static async Task SetTasksToWaitStatus(ITaskRepository taskRepository, CancellationToken cancellationToken) {
    await taskRepository.UpdateAsync(x =>
      x.IsActive &&
      (x.ExecutionStatus != ExecutionStatus.Running && x.ExecutionStatus != ExecutionStatus.Waiting) &&
      (x.NextExecution > DateTimeOffset.UtcNow.AddMinutes(-1) || x.NextExecution == DateTimeOffset.MinValue),
      set => set
      .SetProperty(p => p.ExecutionStatus, ExecutionStatus.Waiting)
      .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow),
      cancellationToken: cancellationToken
    );

    await taskRepository.CommitAsync(cancellationToken);
  }

  private async Task UpdateMissedTasks(ITaskRepository taskRepository, IEnumerable<ScheduledTask> tasks, CancellationToken cancellationToken) {
    if(!tasks.Any())
      return;

    logger.LogWarning("Missed tasks: {missedTasks}", string.Join(", ", tasks.Select(x => x.Id)));
    foreach(var task in tasks) {
      task.ExecutionStatus = ExecutionStatus.Missfire;
      await taskRepository.UpdateAsync(task, cancellationToken: cancellationToken);
      await taskRepository.CommitAsync(cancellationToken);
    }

  }

  private async Task<List<CurrentScheduledTaskInfo>> RefreshData(CancellationToken cancellationToken) {
    try {
      using var scope = serviceProvider.CreateScope();
      var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

      await SetTasksToWaitStatus(taskRepository, cancellationToken);
      var tasks = await taskRepository.GetAllAsync(t => t.ExecutionStatus == ExecutionStatus.Waiting, cancellationToken);

      var filteredList = tasks
        .Where(st => st.AvailableToRun())
        .Select(st => new CurrentScheduledTaskInfo(st, st.Triggers.FirstOrDefault(tg => tg.IsEligibleToRun())?.Id))
        .Where(t => t.TriggerId is not null);

      if(filteredList.Any()) {
        var missed = tasks.ExceptBy(filteredList.Select(x => x.Task.Id), x => x.Id);
        await UpdateMissedTasks(taskRepository, tasks, cancellationToken);
      }

      return [.. filteredList];
    } catch(Exception ex) {
      logger.LogError(ex, "Error while searching for new tasks");
      return [];
    }
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

      await Task.Delay(_config.DatabaseCheckInterval, cancellationToken);
    }
  }

  private async Task ExecuteTaskScopedAsync(CurrentScheduledTaskInfo taskInfo, CancellationToken cancellationToken) {
    using var scope = serviceProvider.CreateScope();
    var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    var historyRepository = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();
    var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

    await ExecuteWithTrackingAsync(taskInfo, taskRepository, historyRepository, triggerRepository, cancellationToken);
  }

  private async Task ExecuteWithTrackingAsync(CurrentScheduledTaskInfo taskInfo, ITaskRepository taskRepository, ITaskHistoryRepository historyRepository, ITriggerRepository triggerRepository, CancellationToken cancellationToken) {
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

      while(retryCount <= _config.MaxRetry) {
        try {
          logger.LogInformation("Execution attempt {retryCount} of {MaxRetry}", retryCount, _config.MaxRetry);
          var result = await job.ExecuteJobAsync(cancellationToken);
          history.EndedAt = DateTimeOffset.UtcNow;
          history.Status = ExecutionStatus.Success;
          history.ResultBlob = TaskSerializer.SerializeObjectToBytes(result);

          taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
          taskInfo.Task.ExecutionStatus = ExecutionStatus.Success;
          logger.LogInformation("Execution attempt {retryCount} status: Success", retryCount);
          break;
        } catch when(_config.RetryOnFailure && retryCount < _config.MaxRetry - 1) {
          logger.LogWarning("Failed. Waiting {RetryWaitDelay} to try again.", _config.RetryWaitDelay);
          retryCount++;
          await Task.Delay(_config.RetryWaitDelay, cancellationToken);
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

      if(baseTrigger.ShouldAutoDelete && baseTrigger.EndsAt.HasValue && baseTrigger.EndsAt > DateTimeOffset.UtcNow) {
        await triggerRepository.DeleteAsync(baseTrigger.Id, cancellationToken: cancellationToken);
      } else {
        baseTrigger.UpdateTriggerState();
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
  }
}
