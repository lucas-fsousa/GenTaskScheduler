namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Trigger for weekly execution on specific days and times
/// </summary>
public class WeeklyTrigger: BaseTrigger {
  /// <summary>
  /// The days of the week on which the task should run. Represented as a comma-separated list (e.g., "Monday,Wednesday").
  /// </summary>
  public string DaysOfWeek { get; set; } = string.Empty;

}

