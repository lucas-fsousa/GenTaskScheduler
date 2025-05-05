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

/// <summary>
/// SchedulerLauncher is responsible for launching the tasks.
/// </summary>
/// <param name="serviceProvider">Dependency provider for internal management and scope creation for dependency injection.</param>
/// <param name="logger">Logger for creating and displaying system logs.</param>
internal class GenSchedulerLauncher(IServiceProvider serviceProvider, ILogger<ApplicationLogger> logger): ISchedulerLauncher {
  private static readonly SchedulerConfiguration _config = GenSchedulerEnvironment.SchedulerConfiguration;
  private static readonly SemaphoreSlim _semaphore = new(_config.MaxTasksDegreeOfParallelism);
  private static readonly GenSchedulerTaskStatus[] _invalidStatusTopUpdate = [GenSchedulerTaskStatus.Running, GenSchedulerTaskStatus.Waiting];

  /// <inheritdoc/>
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

  /// <summary>
  /// Responsible for managing task status. Updates the status to "Waiting" in the window up to 1 minute before the expected date/time for execution.
  /// </summary>
  /// <param name="taskRepository">Task repository instance for data update.</param>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns></returns>
  private static async Task SetTasksToWaitStatus(ITaskRepository taskRepository, CancellationToken cancellationToken) {
    var minRange = DateTimeOffset.UtcNow.AddMinutes(-1);
    var maxRange = DateTimeOffset.UtcNow;
    await taskRepository.UpdateAsync(x =>
      x.IsActive &&
      !_invalidStatusTopUpdate.Contains(x.ExecutionStatus) &&
      x.NextExecution >= minRange && 
      x.NextExecution <= maxRange &&
      x.UpdatedAt < minRange,
      set => set
      .SetProperty(p => p.ExecutionStatus, GenSchedulerTaskStatus.Waiting)
      .SetProperty(p => p.UpdatedAt, minRange),
      cancellationToken: cancellationToken
    );

    await taskRepository.CommitAsync(cancellationToken);
  }

  /// <summary>
  /// RefreshData is responsible for retrieving the tasks that are eligible to run.
  /// </summary>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns><see cref="List{T}"/></returns>
  private async Task<List<CurrentScheduledTaskInfo>> RefreshData(CancellationToken cancellationToken) {
    try {
      using var scope = serviceProvider.CreateScope();
      var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

      await SetTasksToWaitStatus(taskRepository, cancellationToken);
      var tasks = await taskRepository.GetAllAsync(t => t.ExecutionStatus == GenSchedulerTaskStatus.Waiting && t.IsActive, cancellationToken);

      var filteredList = tasks
        .Where(st => st.AvailableToRun())
        .Select(st => new CurrentScheduledTaskInfo(st, st.Triggers.FirstOrDefault(tg => tg.IsEligibleToRun())?.Id))
        .Where(t => t.TriggerId is not null)
        .ToList();

      return [.. filteredList];
    } catch(Exception ex) {
      logger.LogError(ex, "Error while searching for new tasks");
      return [];
    }
  }

  /// <summary>
  /// Execution intermediary, creates the database scope and requests the execution of the task, 
  /// ensuring that there will be no internal interference between the tasks.
  /// </summary>
  /// <param name="taskInfo">Information about the task to be executed</param>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns></returns>
  private async Task ExecuteTaskScopedAsync(CurrentScheduledTaskInfo taskInfo, CancellationToken cancellationToken) {
    using var scope = serviceProvider.CreateScope();
    var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    var historyRepository = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();
    var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

    await ExecuteWithTrackingAsync(taskInfo, taskRepository, historyRepository, triggerRepository, cancellationToken);
  }

  /// <summary>
  /// Ultimate executor who manages the received scope and tracks updates during task execution.
  /// </summary>
  /// <param name="taskInfo">Information about the task to be executed</param>
  /// <param name="taskRepository">Task repository for handling tasks</param>
  /// <param name="historyRepository">Task History Repository for handling history</param>
  /// <param name="triggerRepository">Trigger repository for handling triggers</param>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns></returns>
  private async Task ExecuteWithTrackingAsync(CurrentScheduledTaskInfo taskInfo, ITaskRepository taskRepository, ITaskHistoryRepository historyRepository, ITriggerRepository triggerRepository, CancellationToken cancellationToken) {
    var history = new TaskExecutionHistory {
      TaskId = taskInfo.Task.Id,
      StartedAt = DateTimeOffset.UtcNow,
      TriggerId = taskInfo.TriggerId
    };

    var baseTrigger = taskInfo.Task.Triggers.First(t => t.Id == taskInfo.TriggerId);
    baseTrigger.LastTriggeredStatus = GenTriggerTriggeredStatus.Success;
    try {
      if(taskInfo.Task.ExecutionStatus == GenSchedulerTaskStatus.Running) {
        logger.LogWarning("Task {Id} is already running. Skipping execution.", taskInfo.Task.Id);
        return;
      }

      taskInfo.Task.ExecutionStatus = GenSchedulerTaskStatus.Running;
      await taskRepository.UpdateAsync(taskInfo.Task, cancellationToken: cancellationToken);

      var retryCount = 1;
      var maxRetry = Math.Max(1, _config.MaxRetry);
      var job = TaskSerializer.Deserialize(taskInfo.Task.BlobArgs);
      
      logger.LogInformation("Trigger ({trigger}) with description ({description}) was successfully triggered", baseTrigger.GetType().Name, baseTrigger.TriggerDescription);
      while(retryCount <= maxRetry && !cancellationToken.IsCancellationRequested) {
        try {
          logger.LogInformation("Execution attempt {retryCount} of {MaxRetry}", retryCount, _config.MaxRetry);
          var result = await job.ExecuteJobAsync(cancellationToken);
          history.EndedAt = DateTimeOffset.UtcNow;
          history.Status = GenTaskHistoryStatus.Success;
          history.ResultBlob = TaskSerializer.SerializeObjectToBytes(result);

          taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
          logger.LogInformation("Execution attempt {retryCount} status: Success", retryCount);
          break;
        } catch when(_config.RetryOnFailure && retryCount < maxRetry) {
          logger.LogWarning("Execution attempt {retryCount} status: Failed. Waiting {RetryWaitDelay} to try again.", retryCount, _config.RetryWaitDelay);
          retryCount++;
          await Task.Delay(_config.RetryWaitDelay, cancellationToken);
        } catch(Exception finalEx) {
          logger.LogWarning("Execution attempt {retryCount} status: Failed.", retryCount);
          history.EndedAt = DateTimeOffset.UtcNow;
          history.Status = GenTaskHistoryStatus.Failed;
          history.ErrorMessage = finalEx.Message;

          taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;

          logger.LogError(finalEx, "The execution failed on all attempts.");
          break;
        }
      }
    } catch(Exception ex) {
      history.Status = GenTaskHistoryStatus.Failed;
      history.ErrorMessage = ex.Message;
    } finally {
      taskInfo.Task.ExecutionStatus = GenSchedulerTaskStatus.Ready;
      if(taskInfo.Task.ExecutionStatus == GenSchedulerTaskStatus.Running && cancellationToken.IsCancellationRequested)
        history.Status = GenTaskHistoryStatus.Canceled;
    }

    #region[UPDATE TASK & TRIGGER]
    if(baseTrigger.ShouldAutoDelete && baseTrigger.EndsAt.HasValue && baseTrigger.EndsAt > DateTimeOffset.UtcNow) {
      await triggerRepository.DeleteAsync(baseTrigger.Id, cancellationToken: cancellationToken);
    } else {
      baseTrigger.UpdateTriggerState();
      await triggerRepository.UpdateAsync(baseTrigger, cancellationToken: cancellationToken);
    }

    if(taskInfo.Task.AutoDelete) {
      logger.LogInformation($"Task configured to auto-delete after execution.");
      await taskRepository.DeleteAsync(taskInfo.Task.Id, cancellationToken: cancellationToken);
      logger.LogInformation("Task deleted successfully");
    } else {
      await taskRepository.UpdateAsync(taskInfo.Task, cancellationToken: cancellationToken);
    }

    logger.LogInformation("Saving execution history");
    history.EndedAt = DateTimeOffset.UtcNow;
    await historyRepository.AddAsync(history, cancellationToken: cancellationToken);
    #endregion
  }
}
