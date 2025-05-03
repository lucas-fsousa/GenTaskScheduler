using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs on specific dates and times.
/// </summary>
public class CalendarTrigger: BaseTrigger {
  /// <summary>
  /// List of specific date/times when the task should run.
  /// </summary>
  public List<CalendarEntry> CalendarEntries { get; set; } = [];
}

