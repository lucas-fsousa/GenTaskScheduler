using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Interface for building a weekly trigger.
/// </summary>
public interface IWeeklyTriggerBuild : ITimerOfDayTriggerStep {
  /// <summary>
  /// Sets the days of the week for the trigger.
  /// </summary>
  /// <param name="days">An array of <see cref="DayOfWeek"/> values representing the days of the week.</param>
  /// <returns><see cref="ITimerOfDayTriggerStep"/></returns>
  ITimerOfDayTriggerStep SetDaysOfWeek(params DayOfWeek[] days);
}
