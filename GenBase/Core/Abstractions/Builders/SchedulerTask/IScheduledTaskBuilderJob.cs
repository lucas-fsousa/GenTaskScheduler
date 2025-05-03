namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;

/// <summary>
/// Interface for configuring a scheduled task job.
/// </summary>
public interface IScheduledTaskBuilderJob {
  /// <summary>
  /// Add a trigger to the scheduled task.
  /// </summary>
  /// <param name="value">Flag for setting the value</param>
  /// <returns>IScheduledTaskBuilderOptions</returns>
  IScheduledTaskBuilderOptions SetAutoDelete(bool value);

  /// <summary>
  /// Add a trigger to the scheduled task.
  /// </summary>
  /// <param name="value">Flag for setting the value</param>
  /// <returns>IScheduledTaskBuilderOptions</returns>
  IScheduledTaskBuilderOptions SetIsActive(bool value);

  /// <summary>
  /// Determines whether the configuration is complete
  /// </summary>
  /// <returns>IScheduledTaskBuilderTriggers</returns>
  IScheduledTaskBuilderTriggers Done();
}

