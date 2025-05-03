using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Models.Common;
public class CalendarEntry: BaseModel {

  /// <summary>
  /// The date and time when the entry is scheduled to run.
  /// </summary>
  public DateTimeOffset ScheduledDateTime { get; set; }

  public Guid CalendarTriggerId { get; set; }
  public CalendarTrigger CalendarTrigger { get; set; } = null!;
}
