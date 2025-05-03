using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class TriggerBuilder: IWeeklyTriggerBuilder {
  public IWeeklyTriggerBuilder SetDaysOfWeek(params DayOfWeek[] days) {
    _current!.InternalSetDaysOfWeek(days);
    return this;
  }

  IWeeklyTriggerBuilder IWeeklyTriggerBuilder.SetTimeOfDay(TimeSpan time) {
    _current!.InternalSetTimeOfDay(time);
    return this;
  }
}

