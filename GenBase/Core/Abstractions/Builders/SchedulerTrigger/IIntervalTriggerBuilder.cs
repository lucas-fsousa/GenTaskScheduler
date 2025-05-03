namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
/// <summary>
/// Interface for building an interval trigger.
/// </summary>
public interface IIntervalTriggerBuilder {
  /// <summary>
  /// Sets the initial execution time for the trigger.
  /// </summary>
  /// <param name="initialTime">Initial date/time to fire the trigger</param>
  /// <returns>IIntervalTriggerBuilder</returns>
  IIntervalTriggerBuilder SetInitialExecution(DateTimeOffset initialTime);

  /// <summary>
  /// Sets the repeat interval in minutes for the trigger.
  /// </summary>
  /// <param name="minutes">Time in minutes to repeat trigger firing.</param>
  /// <returns></returns>
  IIntervalTriggerBuilder SetRepeatIntervalMinutes(int minutes);
}