namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Interface for starting the trigger builder process.
/// </summary>
public interface ITriggerBuilderStart {
  /// <summary>
  /// Creates a trigger builder for a once-off execution trigger.
  /// </summary>
  /// <returns>IOnceTriggerBuilder</returns>
  IOnceTriggerBuilder CreateOnceTrigger();

  /// <summary>
  /// Creates a trigger builder for an interval-based execution trigger.
  /// </summary>
  /// <returns>IIntervalTriggerBuilder</returns>
  IIntervalTriggerBuilder CreateIntervalTrigger();

  /// <summary>
  /// Creates a trigger builder for a cron-based execution trigger.
  /// </summary>
  /// <returns>ICronTriggerBuilder</returns>
  ICronTriggerBuilder CreateCronTrigger();

  /// <summary>
  /// Creates a trigger builder for a calendar-based execution trigger.
  /// </summary>
  /// <returns>ICalendarTriggerBuilder</returns>
  ICalendarTriggerBuilder CreateCalendarTrigger();

  /// <summary>
  /// Creates a trigger builder for a daily execution trigger.
  /// </summary>
  /// <returns>IDailyTriggerBuilder</returns>
  IDailyTriggerBuilder CreateDailyTrigger();

  /// <summary>
  /// Creates a trigger builder for a weekly execution trigger.
  /// </summary>
  /// <returns>IWeeklyTriggerBuilder</returns>
  IWeeklyTriggerBuilder CreateWeeklyTrigger();

  /// <summary>
  /// Creates a trigger builder for a monthly execution trigger.
  /// </summary>
  /// <returns>IMonthlyTriggerBuilder</returns>
  IMonthlyTriggerBuilder CreateMonthlyTrigger();
}

