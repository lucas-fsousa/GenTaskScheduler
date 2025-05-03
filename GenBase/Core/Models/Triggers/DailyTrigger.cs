namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Trigger for daily execution at a specific time
/// </summary>
public class DailyTrigger: BaseTrigger {
  /// <summary>
  /// The specific time of day when the task should be executed.
  /// </summary>
  public TimeOnly TimeOfDay { get; set; }

}

