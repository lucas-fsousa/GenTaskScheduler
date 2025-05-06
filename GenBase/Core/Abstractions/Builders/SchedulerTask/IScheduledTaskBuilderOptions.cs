using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;

/// <summary>
/// Interface for configuring options of a scheduled task.
/// </summary>
public interface IScheduledTaskBuilderOptions {

  /// <summary>
  /// Set whether the task should be automatically deleted after completion.
  /// </summary>
  /// <param name="value">Flag value to de set</param>
  /// <returns><see cref="IScheduledTaskBuilderOptions"/></returns>
  IScheduledTaskBuilderOptions SetAutoDelete(bool value);

  /// <summary>
  /// Set whether the task is active or not.
  /// </summary>
  /// <param name="value">Flag value to de set</param>
  /// <returns><see cref="IScheduledTaskBuilderOptions"/></returns>
  IScheduledTaskBuilderOptions SetIsActive(bool value);

  /// <summary>
  /// Determines whether the task will have a maximum execution period.
  /// </summary>
  /// <param name="timeout">Maximum time a task can remain running. This value cannot be <see cref="TimeSpan.Zero"/></param>
  /// <returns><see cref="IScheduledTaskBuilderOptions"/></returns>
  IScheduledTaskBuilderOptions SetTimeout(TimeSpan timeout);

  /// <summary>
  /// Finalize the task configuration and return the built ScheduledTask.
  /// </summary>
  /// <returns>An <see cref="ScheduledTask"/> configured</returns>
  ScheduledTask Build();
}

