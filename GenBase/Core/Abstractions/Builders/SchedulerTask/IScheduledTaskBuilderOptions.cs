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
  /// Finalize the task configuration and return the built ScheduledTask.
  /// </summary>
  /// <returns>An <see cref="ScheduledTask"/> configured</returns>
  ScheduledTask Build();
}

