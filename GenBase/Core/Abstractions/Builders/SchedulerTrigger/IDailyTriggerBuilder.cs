namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Builder for Daily Trigger.
/// </summary>
public interface IDailyTriggerBuilder {
  /// <summary>
  /// Sets the start time of the trigger.
  /// </summary>
  /// <param name="time">Time of day at which execution should begin</param>
  /// <returns>IMonthlyTriggerBuilder</returns>
  IDailyTriggerBuilder SetTimeOfDay(TimeSpan time);
}

