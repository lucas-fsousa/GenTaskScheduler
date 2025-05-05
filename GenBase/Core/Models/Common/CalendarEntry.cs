using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Models.Common;

/// <summary>
/// Represents a calendar entry in the task scheduler.
/// </summary>
public class CalendarEntry: BaseModel {

  /// <summary>
  /// The date and time when the entry is scheduled to run.
  /// </summary>
  public DateTimeOffset ScheduledDateTime { get; set; }

  /// <summary>
  /// Indicates whether this calendar entry has already been executed.
  /// </summary>
  public bool Executed { get; set; }

  /// <summary>
  /// The ID of the scheduled task associated with this calendar entry.
  /// </summary>
  public Guid CalendarTriggerId { get; set; }

  /// <summary>
  /// The scheduled task associated with this calendar entry.
  /// </summary>
  public CalendarTrigger CalendarTrigger { get; set; } = null!;
}
