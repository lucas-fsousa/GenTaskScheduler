using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
/// <summary>
/// Interface for defining a monthly trigger.
/// </summary>
public interface IMonthlyTriggerBuilder {
  /// <summary>
  /// Define specific days of the month for the trigger.
  /// </summary>
  /// <param name="daysOfMonth">Represents the days of the month that will be considered by the trigger.</param>
  /// <returns>IMonthOfYearStepBuilder</returns>
  IMonthOfYearTriggerStep SetDaysOfMonth(params int[] daysOfMonth);
}
