using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
/// <summary>
/// Interface for building an interval trigger.
/// </summary>
public interface IIntervalTriggerBuilder  {
  /// <summary>
  /// Sets the repeat interval in minutes for the trigger.
  /// </summary>
  /// <param name="minutes">Time in minutes to repeat trigger firing.</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetRepeatIntervalMinutes(int minutes);
}