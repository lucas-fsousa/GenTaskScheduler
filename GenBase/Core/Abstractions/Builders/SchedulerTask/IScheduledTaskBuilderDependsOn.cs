using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;

/// <summary>
/// Interface for configuring dependencies of a scheduled task.
/// </summary>
public interface IScheduledTaskBuilderDependsOn {

  /// <summary>
  /// Set the task that this task depends on.
  /// </summary>
  /// <param name="taskId">Id of parent task. This value cannot be empty or null</param>
  /// <returns><see cref="IScheduledTaskBuilderDependsOnWithStatus"/></returns>
  IScheduledTaskBuilderDependsOnWithStatus DependsOn(Guid taskId);

  /// <summary>
  /// Set the task that this task depends on.
  /// </summary>
  /// <param name="task">Parent task. This value cannot be null</param>
  /// <returns><see cref="IScheduledTaskBuilderDependsOnWithStatus"/></returns>
  IScheduledTaskBuilderDependsOnWithStatus DependsOn(ScheduledTask task);

  /// <summary>
  /// Bypass, does not depend on another task for activation
  /// </summary>
  /// <returns><see cref="IScheduledTaskBuilderOptions"/></returns>
  IScheduledTaskBuilderOptions NotDepends();
}

/// <summary>
/// Interface for configuring dependencies of a scheduled task with status.
/// </summary>
public interface IScheduledTaskBuilderDependsOnWithStatus {

  /// <summary>
  /// Set the status of the task that this task depends on.
  /// </summary>
  /// <param name="status">Status indicated in the history of the parent task referring to the last execution. 
  /// If the value is set to <see cref="GenTaskHistoryStatus.None"/>, any status will be valid.</param>
  /// <returns><see cref="IScheduledTaskBuilderOptions"/></returns>
  IScheduledTaskBuilderOptions WithStatus(GenTaskHistoryStatus status);
  /// <summary>
  /// Set the status of the task that this task depends on.
  /// </summary>
  /// <param name="status">Status array indicated in the history of the parent task referring to the last execution.
  /// Cannot be <see cref="GenTaskHistoryStatus.None"/> or Empty</param>
  /// <returns><see cref="IScheduledTaskBuilderOptions"/></returns>
  IScheduledTaskBuilderOptions WithStatus(params GenTaskHistoryStatus[] status);
}

