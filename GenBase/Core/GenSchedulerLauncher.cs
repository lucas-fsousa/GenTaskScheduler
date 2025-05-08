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
using System.Collections.Concurrent;
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
  private readonly ConcurrentQueue<Guid> _taskQueue = new();
  private readonly ConcurrentDictionary<Guid, ScheduledTask> _taskLookup = [];
  private readonly ConcurrentDictionary<Guid, byte> _executingTasks = [];

  /// <inheritdoc/>
  public async Task ExecuteAsync(CancellationToken cancellationToken = default) {
    logger.LogInformation("SchedulerLauncher is starting...");

    while(!cancellationToken.IsCancellationRequested) {
      var executableTasks = await RefreshData(cancellationToken);
      UpdateQueue(executableTasks);
      ProcessQueue(cancellationToken);
      await Task.Delay(_config.DatabaseCheckInterval, cancellationToken);
    }

    logger.LogInformation("SchedulerLauncher is closing...");
  }

  /// <summary>
  /// Updates task definitions after executions.
  /// </summary>
  /// <param name="task">Task to be updated</param>
  /// <param name="cancellationToken">Registration token for request cancellation and immediate termination</param>
  /// <returns></returns>
  private async Task UpdateTaskExecutionInfo(ScheduledTask task, CancellationToken cancellationToken) {
    try {
      using var scope = serviceProvider.CreateScope();
      var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
      var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

      if(task.AutoDelete) {
        logger.LogInformation("TaskId {taskId} configured to auto-delete after execution.", task.Id);
        await taskRepository.DeleteAsync(task.Id, cancellationToken: cancellationToken);
      } else {
        await taskRepository.UpdateAsync(task, cancellationToken: cancellationToken);
      }

      var missedTriggers = task.Triggers.Where(x => x.IsMissedTrigger()).Select(x => x.Id);
      if(!missedTriggers.Any())
        return;

      logger.LogWarning("The triggers with Ids {ids} was lost for the taskId {taskId}", string.Join(", ", missedTriggers), task.Id);

      await triggerRepository.UpdateAsync(f => missedTriggers.Contains(f.Id), set =>
        set.SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow)
        .SetProperty(p => p.LastTriggeredStatus, GenTriggerTriggeredStatus.Missfire.ToString()),
        cancellationToken: cancellationToken
      );
    } catch(Exception ex) {
      logger.LogError(ex, "An error occurred while trying to update task with Id {taskId} definitions", task.Id);
    } finally {
      _taskLookup.TryRemove(task.Id, out _);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="baseTrigger">Trigger to be updated</param>
  /// <param name="cancellationToken">Registration token for request cancellation and immediate termination</param>
  /// <returns></returns>
  private async Task UpdateTriggerExecutionInfo(BaseTrigger? baseTrigger, CancellationToken cancellationToken) {
    try {
      using var scope = serviceProvider.CreateScope();
      var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

      if(baseTrigger is null) {
        logger.LogWarning("No associated triggers were found.");
        return;
      }

      if(baseTrigger.ShouldAutoDelete && baseTrigger.EndsAt.HasValue && baseTrigger.EndsAt < DateTimeOffset.UtcNow) {
        logger.LogInformation("triggerId {triggerId} configured for automatic deletion after expiration date.", baseTrigger.Id);
        await triggerRepository.DeleteAsync(baseTrigger.Id, cancellationToken: cancellationToken);
      } else {
        baseTrigger.UpdateTriggerState();
        await triggerRepository.UpdateAsync(baseTrigger, cancellationToken: cancellationToken);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "An error occurred while trying to update trigger with Id {triggerId}", baseTrigger?.Id);
    }
  }

  /// <summary>
  /// Updates the queue with the most current data.
  /// </summary>
  /// <param name="executables">List of valid tasks to be executed.</param>
  private void UpdateQueue(IEnumerable<ScheduledTask> tasks) {
    var queuedIds = _taskQueue.ToHashSet();
    foreach(var task in tasks) {
      if(queuedIds.Contains(task.Id)) {
        if(!_taskLookup.TryGetValue(task.Id, out var lookupTask) || lookupTask.Equals(task))
          continue;

        _taskLookup[task.Id] = task;
        continue;
      }

      _taskLookup[task.Id] = task;
      _taskQueue.Enqueue(task.Id);
    }
  }

  /// <summary>
  /// Processes the task queue synchronously and maintains async references for cases of cancellation requests via the registration token.
  /// </summary>
  /// <param name="cancellationToken">Registration token for request cancellation and immediate termination</param>
  private void ProcessQueue(CancellationToken cancellationToken) {
    if(_taskQueue.IsEmpty)
      return;

    while(_taskQueue.TryDequeue(out var taskId)) {
      if(!_executingTasks.TryAdd(taskId, 0)) {
        logger.LogWarning("Task {TaskId} is already being processed. Skipping.", taskId);
        continue;
      }
      _ = Task.Run(async () => {
        BaseTrigger? trigger = null;
        logger.LogInformation("TaskId {TaskId} dequeued.", taskId);
        logger.LogInformation("tasks in queue: {tk}, lookup: {lk}", _taskQueue.Count, _taskLookup.Count);

        if(!_taskLookup.TryGetValue(taskId, out var task)) {
          logger.LogWarning("The taskId {taskId} has been removed or changed and is no longer available for execution.", taskId);
          return;
        }

        try {
          using(logger.BeginScope("ID={id}", task.UpdatedAt.Ticks.ToString("x2"))) {
            logger.LogInformation("Task with ID {id} waiting trigger", task.Id);
            logger.LogInformation("expected execution: {expect}", task.NextExecution);

            while(task.NextExecution - DateTimeOffset.UtcNow > TimeSpan.Zero && !cancellationToken.IsCancellationRequested) {
              await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
              trigger = task.Triggers.FirstOrDefault(x => x.IsEligibleToRun());
              if(trigger is not null)
                break;
            }

            if(trigger is null || !task.AvailableToRun()) {
              logger.LogWarning("The reported taskId {taskId} or trigger does not meet the criteria for execution", task.Id);
              task.ExecutionStatus = GenSchedulerTaskStatus.Ready.ToString();
              return;
            }

            if(task.ExecutionStatus == GenSchedulerTaskStatus.Running.ToString()) {
              logger.LogWarning("Task {Id} is already running. Skipping execution.", task.Id);
              return;
            }

            await _semaphore.WaitAsync(cancellationToken);
            var delay = DateTimeOffset.UtcNow - task.NextExecution;
            if(delay > GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance)
              logger.LogWarning("Task executing with delay of {delay}", delay);

            logger.LogInformation("Starting execution of taskId {taskId}", task.Id);
            using var scope = serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var historyRepository = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();
            var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

            await ExecuteWithTrackingAsync(new CurrentScheduledTaskInfo(task, trigger.Id), taskRepository, historyRepository, triggerRepository, cancellationToken);
            logger.LogInformation("Execution of taskId {taskId} completed.", task.Id);
          }
        } catch(Exception ex) {
          logger.LogError(ex, "Unhandled error while executing task {taskId}", taskId);
        } finally {
          await UpdateTaskExecutionInfo(task, cancellationToken);
          await UpdateTriggerExecutionInfo(trigger, cancellationToken);
          _executingTasks.TryRemove(taskId, out _);
          _semaphore.Release();
        }
      }, cancellationToken);
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
  private async Task UpdatePendingAndStuckTasksAsync(ITaskRepository taskRepository, CancellationToken cancellationToken) {
    var now = DateTimeOffset.UtcNow;
    var maxRange = now.AddMinutes(1);
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance + TimeSpan.FromMinutes(1);
    var allTasks = await taskRepository.GetAllAsync(cancellationToken: cancellationToken);
    var inactives = allTasks.Where(x => !x.IsActive).Select(x => x.Id).ToList();

    var readyStatusIds = allTasks
      .Where(x =>
        x.ExecutionStatus == GenSchedulerTaskStatus.Ready.ToString() &&
        x.NextExecution < maxRange &&
        x.NextExecution >= now
      )
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
      readyStatusIds = [.. readyStatusIds.Except(inactives)];
      stuckRunningIds = [.. stuckRunningIds.Except(inactives)];

      logger.LogInformation("AutoDeleteInactiveTasks enabled. Deleting inactive tasks: {ids}", string.Join(", ", inactives));
      await taskRepository.DeleteAsync(x => inactives.Contains(x.Id), false, cancellationToken);
    }

    if(readyStatusIds.Count > 0) {
      await taskRepository.UpdateAsync(x => readyStatusIds.Contains(x.Id),
        set => set
          .SetProperty(p => p.ExecutionStatus, GenSchedulerTaskStatus.Waiting.ToString())
          .SetProperty(p => p.UpdatedAt, now),
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
  /// RefreshData is responsible for retrieving the tasks that are eligible to run.
  /// </summary>
  /// <param name="cancellationToken">Token for managing and propagating the cancellation request.</param>
  /// <returns><see cref="List{T}"/></returns>
  private async Task<List<ScheduledTask>> RefreshData(CancellationToken cancellationToken) {
    try {
      using var scope = serviceProvider.CreateScope();
      var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

      await UpdatePendingAndStuckTasksAsync(taskRepo, cancellationToken);
      return await taskRepo.GetAllAsync(t => t.ExecutionStatus == GenSchedulerTaskStatus.Waiting.ToString() && t.IsActive, cancellationToken);
    } catch(Exception ex) {
      logger.LogError(ex, "Error while refreshing task data");
      return [];
    }
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
        taskInfo.Task.LastExecutionHistoryId = history.Id;

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

      taskInfo.Task.LastExecutionHistoryId = history.Id;
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

      taskInfo.Task.LastExecutionHistoryId = history.Id;
      await historyRepository.AddAsync(history, cancellationToken: cancellationToken);
      logger.LogError(ex, "Error while executing task {Id}", taskInfo.Task.Id);
    } finally {
      taskInfo.Task.ExecutionStatus = GenSchedulerTaskStatus.Ready.ToString();
      stopWatch.Stop();
    }
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
        await job.ExecuteJobAsync(cancellationToken);
        history.EndedAt = DateTimeOffset.UtcNow;
        history.Status = GenTaskHistoryStatus.Success.ToString();

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
