using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
/// <summary>
/// Interface for building a monthly trigger.
/// </summary>
public interface IMonthlyTriggerBuilder {
  /// <summary>
  /// Sets the start time of the trigger. This is the time of day when the trigger will first fire.
  /// </summary>
  /// <param name="time">Time of day at which execution should begin</param>
  /// <returns>IMonthlyTriggerBuilder</returns>
  IMonthlyTriggerBuilder SetTimeOfDay(TimeSpan time);

  /// <summary>
  /// Define specific days of the month for the trigger.
  /// </summary>
  /// <param name="daysOfMonth">Represents the days of the month that will be considered by the trigger.</param>
  /// <returns>IMonthlyTriggerBuilder</returns>
  IMonthlyTriggerBuilder SetDaysOfMonth(params int[] daysOfMonth);

  /// <summary>
  /// Define specific months of the year for the trigger.
  /// </summary>
  /// <param name="monthOfYears">Defines the months of the year in which the trigger is valid for execution</param>
  /// <returns>IMonthlyTriggerBuilder</returns>
  IMonthlyTriggerBuilder SetMonthsOfYear(params MonthOfYear[] monthOfYears);
}

