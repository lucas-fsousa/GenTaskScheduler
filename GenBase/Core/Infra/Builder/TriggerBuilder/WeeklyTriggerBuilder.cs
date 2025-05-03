using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class GenSchedulerTriggerBuilder: IWeeklyTriggerBuild {
  public IWeeklyTriggerBuild CreateWeeklyTrigger(DateTimeOffset startDate) {
    _current = new WeeklyTrigger();
    _current!.InternalSetStartDate(startDate);
    return this;
  }

  public ITimerOfDayTriggerStep SetDaysOfWeek(params DayOfWeek[] days) {
    _current!.InternalSetDaysOfWeek(days);
    return this;
  }

  public ICommonTriggerStep SetTimeOfDay(TimeOnly time) {
    _current!.InternalSetTimeOfDay(time);
    return this;
  }
}

