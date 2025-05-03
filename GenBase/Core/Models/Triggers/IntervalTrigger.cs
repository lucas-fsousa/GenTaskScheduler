using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs at a specified start time and repeats after a defined interval.
/// </summary>
public class IntervalTrigger: BaseTrigger {
  /// <summary>
  /// The interval in minutes between each execution of the task.
  /// </summary>
  public int RepeatIntervalMinutes { get; set; }

  /// <summary>
  /// The time of day when the task should be executed.
  /// </summary>
  public DateTimeOffset InitialExecutionTime { get; set; }
}

