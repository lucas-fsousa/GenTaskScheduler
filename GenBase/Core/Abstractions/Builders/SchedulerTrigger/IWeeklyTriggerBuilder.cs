namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

public interface IWeeklyTriggerBuilder {
  IWeeklyTriggerBuilder SetTimeOfDay(TimeSpan time);
  IWeeklyTriggerBuilder SetDaysOfWeek(params DayOfWeek[] days);
}
