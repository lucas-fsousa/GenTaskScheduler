using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Models.Common;
public class CalendarEntry: BaseModel {

  /// <summary>
  /// The date and time when the entry is scheduled to run.
  /// </summary>
  public DateTimeOffset ScheduledDateTime { get; set; }

  /// <summary>
  /// Indicates whether this calendar entry has already been executed.
  /// </summary>
  public bool Executed { get; set; }

  public Guid CalendarTriggerId { get; set; }
  public CalendarTrigger CalendarTrigger { get; set; } = null!;
}
