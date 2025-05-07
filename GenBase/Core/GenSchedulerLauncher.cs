using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace GenTaskScheduler.Core;

/// <summary>
/// SchedulerLauncher is responsible for launching the tasks.
/// </summary>
/// <param name="serviceProvider">Dependency provider for internal management and scope creation for dependency injection.</param>
/// <param name="logger">Logger for creating and displaying system logs.</param>
internal class GenSchedulerLauncher(IServiceProvider serviceProvider, ILogger<ApplicationLogger> logger): ISchedulerLauncher {
  private static readonly SchedulerConfiguration _config = GenSchedulerEnvironment.SchedulerConfiguration;
  private static readonly SemaphoreSlim _semaphore = new(_config.MaxTasksDegreeOfParallelism);
  private static readonly string[] _invalidStatusTopUpdate = [GenSchedulerTaskStatus.Running.ToString(), GenSchedulerTaskStatus.Waiting.ToString()];

  /// <inheritdoc/>
  public async Task ExecuteAsync(CancellationToken cancellationToken = default) {
    logger.LogInformation("SchedulerLauncher is starting...");
    while(!cancellationToken.IsCancellationRequested) {
      var executableTasks = await RefreshData(cancellationToken);

      foreach(var taskInfo in executableTasks) {
        _ = Task.Run(async () => {
          try {
            await _semaphore.WaitAsync(cancellationToken);
            using(logger.BeginScope("ID={id}", taskInfo.Task.UpdatedAt.Ticks.ToString("x2"))) {
              var delay = DateTimeOffset.UtcNow - taskInfo.Task.NextExecution;
              if(delay > GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance)
                logger.LogWarning("Task executing with delay of {delay}", delay);
              
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
  /// Responsible for managing task status. 
  /// Updates the status to "Waiting" in the window up to 1 minute before the expected date/time for execution.
  /// Updates the status to "Ready" if the task is stuck for some reason, thus preventing a new execution from happening.
  /// </summary>
  /// <param name="taskRepository">Task repository instance for data update.</param>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns></returns>
  private async Task InternalRequiredUpdateTaskStatus(ITaskRepository taskRepository, CancellationToken cancellationToken) {
    var now = DateTimeOffset.UtcNow;
    var minRange = now.AddMinutes(-1);
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance + TimeSpan.FromMinutes(1);
    var allTasks = await taskRepository.GetAllAsync(cancellationToken: cancellationToken);
    var inactives = allTasks.Where(x => !x.IsActive).Select(x => x.Id).ToList();

    var waitingStatusIds = allTasks
      .Where(x =>
        !_invalidStatusTopUpdate.Contains(x.ExecutionStatus) &&
        x.NextExecution >= minRange &&
        x.NextExecution <= now &&
        x.UpdatedAt < minRange)
      .Select(x => x.Id)
      .ToList();

    var stuckRunningIds = allTasks
      .Where(x =>
        x.ExecutionStatus == GenSchedulerTaskStatus.Running.ToString() &&
        (
          (x.MaxExecutionTime != TimeSpan.Zero && now - x.LastExecution > x.MaxExecutionTime + tolerance) ||
          (x.MaxExecutionTime == TimeSpan.Zero && x.NextExecution < now - tolerance)
        ))
      .Select(x => x.Id)
      .ToList();

    if(_config.AutoDeleteInactiveTasks && inactives.Count > 0) {
      waitingStatusIds = [.. waitingStatusIds.Except(inactives)];
      stuckRunningIds = [.. stuckRunningIds.Except(inactives)];

      logger.LogInformation("AutoDeleteInactiveTasks enabled. Deleting inactive tasks: {ids}", string.Join(", ", inactives));
      await taskRepository.DeleteAsync(x => inactives.Contains(x.Id), false, cancellationToken);
    }
    
    if(waitingStatusIds.Count > 0) {
      await taskRepository.UpdateAsync(x => waitingStatusIds.Contains(x.Id),
        set => set
          .SetProperty(p => p.ExecutionStatus, GenSchedulerTaskStatus.Waiting.ToString())
          .SetProperty(p => p.UpdatedAt, minRange),
        false,
        cancellationToken
      );
    }

    if(stuckRunningIds.Count > 0) {
      await taskRepository.UpdateAsync(x => stuckRunningIds.Contains(x.Id),
        set => set
          .SetProperty(p => p.ExecutionStatus, GenSchedulerTaskStatus.Ready.ToString())
          .SetProperty(p => p.UpdatedAt, now),
        false,
        cancellationToken
      );
    }

    await taskRepository.CommitAsync(cancellationToken);
  }

  /// <summary>
  /// Task responsible for managing the status of triggers. 
  /// Updates the status to <see cref="GenTriggerTriggeredStatus.Missfire"/> for triggers that are eligible to run but have not been executed.
  /// </summary>
  /// <param name="triggerRepository">Trigger repository instance for data update.</param>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns></returns>
  private static async Task SetTriggersToMissedStatus(List<BaseTrigger> triggers, ITriggerRepository triggerRepository, CancellationToken cancellationToken) {
    var triggerIds = triggers.Select(x => x.Id).ToList();

    if(triggerIds.Count == 0)
      return;

    await triggerRepository.UpdateAsync(f => triggerIds.Contains(f.Id), set =>
      set.SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow)
      .SetProperty(p => p.LastTriggeredStatus, GenTriggerTriggeredStatus.Missfire.ToString()),
      cancellationToken: cancellationToken
    );
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
      var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

      await InternalRequiredUpdateTaskStatus(taskRepository, cancellationToken);
      var tasks = await taskRepository.GetAllAsync(t => t.ExecutionStatus == GenSchedulerTaskStatus.Waiting.ToString() && t.IsActive, cancellationToken);

      var filteredList = tasks
        .Where(st => st.AvailableToRun())
        .Select(st => new CurrentScheduledTaskInfo(st, st.Triggers.FirstOrDefault(tg => tg.IsEligibleToRun())?.Id))
        .Where(t => t.TriggerId is not null)
        .ToList();

      var missedTriggers = tasks
        .ExceptBy(filteredList.Select(x => x.Task.Id), x => x.Id)
        .SelectMany(t => t.Triggers)
        .Where(t => t.IsMissedTrigger())
        .ToList();

      await SetTriggersToMissedStatus(missedTriggers, triggerRepository, cancellationToken);
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
    var baseTrigger = taskInfo.Task.Triggers.First(t => t.Id == taskInfo.TriggerId);

    if(taskInfo.Task.ExecutionStatus == GenSchedulerTaskStatus.Running.ToString()) {
      logger.LogWarning("Task {Id} is already running. Skipping execution.", taskInfo.Task.Id);
      return;
    }

    var stopWatch = new Stopwatch();
    try {
      stopWatch.Start();
      taskInfo.Task.ExecutionStatus = GenSchedulerTaskStatus.Running.ToString();
      taskInfo.Task.LastExecution = DateTimeOffset.UtcNow;
      await taskRepository.UpdateAsync(taskInfo.Task, cancellationToken: cancellationToken);

      baseTrigger.LastTriggeredStatus = GenTriggerTriggeredStatus.Success.ToString();
      await triggerRepository.UpdateAsync(baseTrigger, cancellationToken: cancellationToken);

      var job = TaskSerializer.Deserialize(taskInfo.Task.BlobArgs);

      logger.LogInformation("Trigger ({trigger}) with description ({description}) was successfully triggered", baseTrigger.GetType().Name, baseTrigger.TriggerDescription);

      while(baseTrigger.IsEligibleToRun() && !cancellationToken.IsCancellationRequested) {
        var history = await RecurringExecution(taskInfo, job, cancellationToken);
        logger.LogInformation("Saving execution history");
        await historyRepository.AddAsync(history, cancellationToken: cancellationToken);
        baseTrigger.UpdateTriggerState();
        await Task.Delay(baseTrigger.ExecutionInterval ?? TimeSpan.Zero, cancellationToken);
        if(stopWatch.Elapsed > taskInfo.Task.MaxExecutionTime && taskInfo.Task.MaxExecutionTime != TimeSpan.Zero)
          throw new TimeoutException("Aborted task execution due to timeout");
      }
    } catch(TimeoutException timeoutEx) {
      var history = new TaskExecutionHistory {
        TaskId = taskInfo.Task.Id,
        TriggerId = taskInfo.TriggerId,
        StartedAt = DateTimeOffset.UtcNow,
        EndedAt = DateTimeOffset.UtcNow,
        Status = GenTaskHistoryStatus.Canceled.ToString(),
        ErrorMessage = timeoutEx.Message
      };

      await historyRepository.AddAsync(history, cancellationToken: cancellationToken);
      logger.LogCritical(timeoutEx, "Task execution exceeded the maximum time of {timeout}. Task will be canceled.", taskInfo.Task.MaxExecutionTime);
    } catch(Exception ex) {
      var history = new TaskExecutionHistory {
        TaskId = taskInfo.Task.Id,
        TriggerId = taskInfo.TriggerId,
        StartedAt = DateTimeOffset.UtcNow,
        EndedAt = DateTimeOffset.UtcNow,
        Status = cancellationToken.IsCancellationRequested ? GenTaskHistoryStatus.Canceled.ToString() : GenTaskHistoryStatus.Failed.ToString(),
        ErrorMessage = ex.Message
      };

      await historyRepository.AddAsync(history, cancellationToken: cancellationToken);
      logger.LogError(ex, "Error while executing task {Id}", taskInfo.Task.Id);
    } finally {
      taskInfo.Task.ExecutionStatus = GenSchedulerTaskStatus.Ready.ToString();
      stopWatch.Stop();
    }

    #region[UPDATE TRIGGER]
    if(baseTrigger.ShouldAutoDelete && baseTrigger.EndsAt.HasValue && baseTrigger.EndsAt < DateTimeOffset.UtcNow) {
      await triggerRepository.DeleteAsync(baseTrigger.Id, cancellationToken: cancellationToken);
    } else {
      baseTrigger.UpdateTriggerState();
      await triggerRepository.UpdateAsync(baseTrigger, cancellationToken: cancellationToken);
    }
    #endregion

    #region[UPDATE TASK]
    if(taskInfo.Task.AutoDelete) {
      logger.LogInformation($"Task configured to auto-delete after execution.");
      await taskRepository.DeleteAsync(taskInfo.Task.Id, cancellationToken: cancellationToken);
    } else {
      await taskRepository.UpdateAsync(taskInfo.Task, cancellationToken: cancellationToken);
    }
    #endregion
  }

  /// <summary>
  /// A helper to handle recurring runs that need individual tracking.
  /// </summary>
  /// <param name="taskInfo">The scheduled task information</param>
  /// <param name="job">The Job to be executed</param>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns><see cref="TaskExecutionHistory"/></returns>
  private async Task<TaskExecutionHistory> RecurringExecution(CurrentScheduledTaskInfo taskInfo, IJob job, CancellationToken cancellationToken) {
    var retryCount = 1;
    var maxRetry = Math.Max(1, _config.MaxRetry);
    var history = new TaskExecutionHistory {
      TaskId = taskInfo.Task.Id,
      StartedAt = DateTimeOffset.UtcNow,
      TriggerId = taskInfo.TriggerId
    };

    while(retryCount <= maxRetry && !cancellationToken.IsCancellationRequested) {
      try {
        logger.LogInformation("Execution attempt {retryCount} of {MaxRetry}", retryCount, _config.MaxRetry);
        var result = await job.ExecuteJobAsync(cancellationToken);
        history.EndedAt = DateTimeOffset.UtcNow;
        history.Status = GenTaskHistoryStatus.Success.ToString();
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
        history.Status = GenTaskHistoryStatus.Failed.ToString();
        history.ErrorMessage = finalEx.Message;

        taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
        logger.LogError(finalEx, "The execution failed on all attempts.");
        break;
      }
    }

    history.EndedAt = DateTimeOffset.UtcNow;
    return history;
  }
}
