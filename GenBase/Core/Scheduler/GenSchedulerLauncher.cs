using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Models.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Scheduler;

/// <summary>
/// SchedulerLauncher is responsible for launching the tasks.
/// </summary>
/// <param name="serviceProvider">Dependency provider for internal management and scope creation for dependency injection.</param>
/// <param name="logger">Logger for creating and displaying system logs.</param>
internal class GenSchedulerLauncher(IServiceProvider serviceProvider, ILogger<ApplicationLogger> logger): ISchedulerLauncher {
  private static readonly SchedulerConfiguration _config = GenSchedulerEnvironment.SchedulerConfiguration;

  /// <inheritdoc/>
  public async Task ExecuteAsync(CancellationToken cancellationToken = default) {
    logger.LogInformation("SchedulerLauncher is starting...");

    var queueProcess = InternalQueueProcess.Create(serviceProvider, logger);
    while(!cancellationToken.IsCancellationRequested) {
      var executableTasks = await RefreshData(cancellationToken);
      queueProcess.IncludeOnQueue(executableTasks, cancellationToken);
      await Task.Delay(_config.DatabaseCheckInterval, cancellationToken);
    }

    logger.LogInformation("SchedulerLauncher is closing...");
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
          x.MaxExecutionTime != TimeSpan.Zero && now - x.LastExecution > x.MaxExecutionTime + tolerance ||
          x.MaxExecutionTime == TimeSpan.Zero && x.NextExecution < now - tolerance
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
}
