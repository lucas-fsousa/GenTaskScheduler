using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs based on specific month and a specific time of day.
/// </summary>
public class MonthlyTrigger: BaseTrigger {
  /// <summary>
  /// The specific time of day when the task should be executed.
  /// </summary>
  public TimeOnly TimeOfDay { get; set; }

  /// <summary>
  /// The days of the month on which the task should run. Represented as a comma-separated list (e.g., "1,15,30").
  /// </summary>
  public string DaysOfMonth { get; set; } = string.Empty;

  /// <summary>
  /// The months of the year when the task should run (e.g., "1,7,12" for January, July, and December).
  /// </summary>
  public string MonthsOfYear { get; set; } = string.Empty;
}

