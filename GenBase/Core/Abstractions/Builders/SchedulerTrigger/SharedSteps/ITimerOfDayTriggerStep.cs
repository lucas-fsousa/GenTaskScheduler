namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

/// <summary>
/// Interface for defining the trigger firing time.
/// </summary>
public interface ITimerOfDayTriggerStep {
  /// <summary>
  /// Sets the start time of the trigger. This is the time of day when the trigger will first fire.
  /// </summary>
  /// <param name="time">Time of day at which execution should begin</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetTimeOfDay(TimeOnly time);
}

