using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class GenSchedulerTriggerBuilder: ICalendarTriggerBuilder {

  ///<inheritdoc/>
  public ICalendarTriggerBuilder CreateCalendarTrigger(DateTimeOffset startDate) {
    _current = new CalendarTrigger();
    _current!.InternalSetStartDate(startDate);
    return this;
  }

  ///<inheritdoc/>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  public ICommonTriggerStep AddCalendarEntries(List<CalendarEntry> entries) {
    if(entries == null || entries.Count == 0)
      throw new ArgumentException("Calendar entries cannot be null or empty", nameof(entries));
    
    if(entries.Any(e => e == null || e.ScheduledDateTime < DateTimeOffset.UtcNow))
      throw new ArgumentException("Calendar entries must be valid and not in the past", nameof(entries));

    if(_current is CalendarTrigger ct)
      ct.CalendarEntries.AddRange(entries);
    else
      throw new InvalidOperationException("Current trigger is not a CalendarTrigger.");

    return this;
  }

  ///<inheritdoc/>
  public ICommonTriggerStep AddCalendarEntry(CalendarEntry entry) {
    AddCalendarEntries([entry]);
    return this;
  }
}

