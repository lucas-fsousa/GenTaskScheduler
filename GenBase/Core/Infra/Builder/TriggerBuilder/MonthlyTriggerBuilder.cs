using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class TriggerBuilder: IMonthlyTriggerBuilder {
  public IMonthlyTriggerBuilder SetDaysOfMonth(params int[] daysOfMonth) {
    _current!.InternalSetDaysOfMonth(daysOfMonth);
    return this;
  }

  public IMonthlyTriggerBuilder SetMonthsOfYear(params MonthOfYear[] monthOfYears) {
    _current!.InternalSetMonthsOfYear(monthOfYears);
    return this;
  }

  public IMonthlyTriggerBuilder SetTimeOfDay(TimeSpan time) {
    _current!.InternalSetTimeOfDay(time);
    return this;
  }
}

