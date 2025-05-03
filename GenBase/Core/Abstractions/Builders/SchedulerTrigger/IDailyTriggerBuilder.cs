using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Builder for Daily Trigger.
/// </summary>
public interface IDailyTriggerBuilder {
  /// <summary>
  /// Sets the start time of the trigger.
  /// </summary>
  /// <param name="time">Time of day at which execution should begin</param>
  /// <returns>ICommonTriggerStep</returns>
  ICommonTriggerStep SetTimeOfDay(TimeOnly time);
}

