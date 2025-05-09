using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GenTaskScheduler.Core.Scheduler;
internal class InternalQueueProcess {
  private static readonly SchedulerConfiguration _config = GenSchedulerEnvironment.SchedulerConfiguration;
  private static readonly SemaphoreSlim _semaphore = new(_config.MaxTasksDegreeOfParallelism);
  private readonly SchedulerQueue<ScheduledTask> _queue;
  private readonly ConcurrentDictionary<Guid, byte> _runningTasks = [];
  private readonly ILogger _logger;
  private readonly IServiceProvider _serviceProvider;

  private InternalQueueProcess(IServiceProvider serviceProvider, ILogger logger) {
    _queue = new(f => f.Id);
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  public static InternalQueueProcess Create(IServiceProvider serviceProvider, ILogger logger) => new(serviceProvider, logger);

  public void IncludeOnQueue(IEnumerable<ScheduledTask> scheduledTasks, CancellationToken cancellationToken) {
    UpdateQueue(scheduledTasks);
    ProcessQueue(cancellationToken);
  }

  /// <summary>
  /// Updates task definitions after executions.
  /// </summary>
  /// <param name="task">Task to be updated</param>
  /// <param name="cancellationToken">Registration token for request cancellation and immediate termination</param>
  /// <returns></returns>
  private async Task UpdateTaskExecutionInfo(ScheduledTask task, CancellationToken cancellationToken) {
    try {
      using var scope = _serviceProvider.CreateScope();
      var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
      var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

      if(task.AutoDelete) {
        _logger.LogInformation("TaskId {taskId} configured to auto-delete after execution.", task.Id);
        await taskRepository.DeleteAsync(task.Id, cancellationToken: cancellationToken);
      } else {
        await taskRepository.UpdateAsync(task, cancellationToken: cancellationToken);
      }

      var missedTriggers = task.Triggers.Where(x => x.IsMissedTrigger()).Select(x => x.Id);
      if(!missedTriggers.Any())
        return;

      _logger.LogWarning("The triggers with Ids {ids} was lost for the taskId {taskId}", string.Join(", ", missedTriggers), task.Id);

      await triggerRepository.UpdateAsync(f => missedTriggers.Contains(f.Id), set =>
        set.SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow)
        .SetProperty(p => p.LastTriggeredStatus, GenTriggerTriggeredStatus.Missfire.ToString()),
        cancellationToken: cancellationToken
      );
    } catch(Exception ex) {
      _logger.LogError(ex, "An error occurred while trying to update task with Id {taskId} definitions", task.Id);
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
      using var scope = _serviceProvider.CreateScope();
      var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

      if(baseTrigger is null) {
        _logger.LogWarning("No associated triggers were found.");
        return;
      }

      if(baseTrigger.ShouldAutoDelete && baseTrigger.EndsAt.HasValue && baseTrigger.EndsAt < DateTimeOffset.UtcNow) {
        _logger.LogInformation("triggerId {triggerId} configured for automatic deletion after expiration date.", baseTrigger.Id);
        await triggerRepository.DeleteAsync(baseTrigger.Id, cancellationToken: cancellationToken);
      } else {
        baseTrigger.UpdateTriggerState();
        await triggerRepository.UpdateAsync(baseTrigger, cancellationToken: cancellationToken);
      }
    } catch(Exception ex) {
      _logger.LogError(ex, "An error occurred while trying to update trigger with Id {triggerId}", baseTrigger?.Id);
    }
  }

  /// <summary>
  /// Updates the queue with the most current data.
  /// </summary>
  /// <param name="tasks">List of valid tasks to be executed.</param>
  private void UpdateQueue(IEnumerable<ScheduledTask> tasks) {
    foreach(var task in tasks) {
      if(!_queue.Contains(task.Id)) {
        _queue.Enqueue(task);
        continue;
      }

      if(_queue.CompareEquals(task))
        continue;

      _queue.TryDequeue(out _);
      _queue.Enqueue(task);
    }
  }

  /// <summary>
  /// Processes the task queue synchronously and maintains async references for cases of cancellation requests via the registration token.
  /// </summary>
  /// <param name="cancellationToken">Registration token for request cancellation and immediate termination</param>
  private void ProcessQueue(CancellationToken cancellationToken) {
    while(_queue.TryDequeue(out var task)) {
      if(task is null) {
        _logger.LogWarning("The current task in the queue has been removed or changed and is no longer available.");
        return;
      }

      if(!_runningTasks.TryAdd(task.Id, 0))
        return;

      _ = Task.Run(async () => {
        BaseTrigger? trigger = null;
        try {
          using(_logger.BeginScope("ID={id}", task.UpdatedAt.Ticks.ToString("x2"))) {
            _logger.LogInformation("Task with ID {id} waiting trigger", task.Id);
            _logger.LogInformation("expected execution: {expect}", task.NextExecution);

            while(task.NextExecution - DateTimeOffset.UtcNow > TimeSpan.Zero && !cancellationToken.IsCancellationRequested) {
              await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
              trigger = task.Triggers.FirstOrDefault(x => x.IsEligibleToRun());
              if(trigger is not null)
                break;
            }

            if(trigger is null || !task.AvailableToRun()) {
              _logger.LogWarning("The reported taskId {taskId} or trigger does not meet the criteria for execution", task.Id);
              task.ExecutionStatus = GenSchedulerTaskStatus.Ready.ToString();
              return;
            }

            if(task.ExecutionStatus == GenSchedulerTaskStatus.Running.ToString()) {
              _logger.LogWarning("Task {Id} is already running. Skipping execution.", task.Id);
              return;
            }

            await _semaphore.WaitAsync(cancellationToken);
            var delay = DateTimeOffset.UtcNow - task.NextExecution;
            if(delay > GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance)
              _logger.LogWarning("Task executing with delay of {delay}", delay);

            _logger.LogInformation("Starting execution of taskId {taskId}", task.Id);
            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var historyRepository = scope.ServiceProvider.GetRequiredService<ITaskHistoryRepository>();
            var triggerRepository = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

            await ExecuteWithTrackingAsync(new CurrentScheduledTaskInfo(task, trigger.Id), taskRepository, historyRepository, triggerRepository, cancellationToken);
            _logger.LogInformation("Execution of taskId {taskId} completed.", task.Id);
          }
        } catch(Exception ex) {
          _logger.LogError(ex, "Unhandled error while executing task {taskId}", task.Id);
        } finally {
          await UpdateTaskExecutionInfo(task, cancellationToken);
          await UpdateTriggerExecutionInfo(trigger, cancellationToken);
          _semaphore.Release();
          _runningTasks.TryRemove(task.Id, out _);
        }
      }, cancellationToken);
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

      _logger.LogInformation("Trigger ({trigger}) with description ({description}) was successfully triggered", baseTrigger.GetType().Name, baseTrigger.TriggerDescription);

      while(baseTrigger.IsEligibleToRun() && !cancellationToken.IsCancellationRequested) {
        var history = await RecurringExecution(taskInfo, job, cancellationToken);
        taskInfo.Task.LastExecutionHistoryId = history.Id;

        _logger.LogInformation("Saving execution history");

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
      _logger.LogCritical(timeoutEx, "Task execution exceeded the maximum time of {timeout}. Task will be canceled.", taskInfo.Task.MaxExecutionTime);
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
      _logger.LogError(ex, "Error while executing task {Id}", taskInfo.Task.Id);
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
        _logger.LogInformation("Execution attempt {retryCount} of {MaxRetry}", retryCount, _config.MaxRetry);
        await job.ExecuteJobAsync(cancellationToken);
        history.EndedAt = DateTimeOffset.UtcNow;
        history.Status = GenTaskHistoryStatus.Success.ToString();

        taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
        _logger.LogInformation("Execution attempt {retryCount} status: Success", retryCount);
        break;
      } catch when(_config.RetryOnFailure && retryCount < maxRetry) {
        _logger.LogWarning("Execution attempt {retryCount} status: Failed. Waiting {RetryWaitDelay} to try again.", retryCount, _config.RetryWaitDelay);
        retryCount++;
        await Task.Delay(_config.RetryWaitDelay, cancellationToken);
      } catch(Exception finalEx) {
        _logger.LogWarning("Execution attempt {retryCount} status: Failed.", retryCount);
        history.EndedAt = DateTimeOffset.UtcNow;
        history.Status = GenTaskHistoryStatus.Failed.ToString();
        history.ErrorMessage = finalEx.Message;

        taskInfo.Task.UpdatedAt = DateTimeOffset.UtcNow;
        _logger.LogError(finalEx, "The execution failed on all attempts.");
        break;
      }
    }

    history.EndedAt = DateTimeOffset.UtcNow;
    return history;
  }
}

