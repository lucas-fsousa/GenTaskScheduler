using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;

/// <summary>
/// Interface for the step to set the months of the year in a monthly trigger.
/// </summary>
public interface IMonthOfYearTriggerStep {

  /// <summary>
  /// Sets the months of the year for the trigger.
  /// </summary>
  /// <param name="monthOfYears">An array of <see cref="MonthOfYear"/> containing the months of the year to be considered by the trigger</param>
  /// <returns><see cref="ITimerOfDayTriggerStep"/></returns>
  ITimerOfDayTriggerStep SetMonthsOfYear(params MonthOfYear[] monthOfYears);
}

