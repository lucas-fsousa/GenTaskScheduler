using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
/// <summary>
/// Interface for defining a monthly trigger.
/// </summary>
public interface IMonthlyTriggerBuilder {
  /// <summary>
  /// Define specific days of the month for the trigger.
  /// </summary>
  /// <param name="daysOfMonth">
  ///   Represents the days of the month that will be considered by the trigger. Days of month must be between 1 and 31.
  ///   Use 0 to indicate the last day of the month.
  ///   If you need something more specific, consider using a CalendarTrigger
  /// </param>
  /// <returns><see cref="IMonthOfYearTriggerStep"/></returns>
  IMonthOfYearTriggerStep SetDaysOfMonth(params int[] daysOfMonth);

  /// <summary>
  /// Define specific days of the month for the trigger.
  /// </summary>
  /// <param name="daysOfMonth">
  ///   Represents the days of the month that will be considered by the trigger. Days of month must be between 1 and 31.
  ///   Use <see cref="IntRange.Zero"/> to indicate the last day of the month.
  ///   If you need something more specific, consider using a CalendarTrigger
  /// </param>
  /// <returns><see cref="IMonthOfYearTriggerStep"/></returns>
  IMonthOfYearTriggerStep SetDaysOfMonth(params IntRange[] ranges);

}
