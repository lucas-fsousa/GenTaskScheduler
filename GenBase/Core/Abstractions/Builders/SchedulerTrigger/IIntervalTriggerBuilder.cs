using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
/// <summary>
/// Interface for building an interval trigger.
/// </summary>
public interface IIntervalTriggerBuilder  {
  /// <summary>
  /// Sets the repeat interval in minutes for the trigger.
  /// </summary>
  /// <param name="minutes">Time in minutes to repeat trigger firing. The range value allowed is 1-59</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetRepeatIntervalMinutes(int minutes);

  /// <summary>
  /// Sets the repeat interval in hours for the trigger.
  /// </summary>
  /// <param name="hours">Time in hours to repeat trigger firing. The range value allowed is 1-23</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetRepeatIntervalHours(int hours);
}