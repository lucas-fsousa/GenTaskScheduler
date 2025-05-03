using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Builder for Calendar Trigger.
/// </summary>
public interface ICalendarTriggerBuilder {
  /// <summary>
  /// Adds a calendar entry to the trigger.
  /// </summary>
  /// <param name="entry">A simple calendar information representing date/time for specific execution</param>
  /// <returns>ICommonTriggerStep</returns>
  ICommonTriggerStep AddCalendarEntry(CalendarEntry entry);

  /// <summary>
  /// Adds a list of calendar entries to the trigger.
  /// </summary>
  /// <param name="entries">List of calendar information representing date/time for specific execution</param>
  /// <returns>ICommonTriggerStep</returns>
  ICommonTriggerStep AddCalendarEntries(List<CalendarEntry> entries);
}
