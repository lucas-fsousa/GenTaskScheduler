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
  /// <returns>IScheduledTaskBuilderDependsOnWithStatus</returns>
  IScheduledTaskBuilderDependsOnWithStatus DependsOn(Guid taskId);

  /// <summary>
  /// Set the task that this task depends on.
  /// </summary>
  /// <param name="task">Parent task. This value cannot be null</param>
  /// <returns>IScheduledTaskBuilderDependsOnWithStatus</returns>
  IScheduledTaskBuilderDependsOnWithStatus DependsOn(ScheduledTask task);

  /// <summary>
  /// Bypass, does not depend on another task for activation
  /// </summary>
  /// <returns>IScheduledTaskBuilderOptions</returns>
  IScheduledTaskBuilderOptions NotDepends();
}

/// <summary>
/// Interface for configuring dependencies of a scheduled task with status.
/// </summary>
public interface IScheduledTaskBuilderDependsOnWithStatus {

  /// <summary>
  /// Set the status of the task that this task depends on.
  /// </summary>
  /// <param name="status">Parent last execution status, the value cannot be None</param>
  /// <returns>IScheduledTaskBuilderOptions</returns>
  IScheduledTaskBuilderOptions WithStatus(ExecutionStatus status);
}

