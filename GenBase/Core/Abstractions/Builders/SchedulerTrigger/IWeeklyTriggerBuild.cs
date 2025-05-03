using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

public interface IWeeklyTriggerBuild : ITimerOfDayTriggerStep {
  ITimerOfDayTriggerStep SetDaysOfWeek(params DayOfWeek[] days);
}
