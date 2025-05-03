using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Cron;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;

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
  /// <param name="startDate">Initial start date</param>
  /// <returns>IIntervalTriggerBuilder</returns>
  IIntervalTriggerBuilder CreateIntervalTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a cron-based execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns>ICronTriggerBuilder</returns>
  ICronExpressionTriggerStep CreateCronTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a calendar-based execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns>ICalendarTriggerBuilder</returns>
  ICalendarTriggerBuilder CreateCalendarTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a daily execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns>IDailyTriggerBuilder</returns>
  IDailyTriggerBuilder CreateDailyTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a weekly execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns>IWeeklyTriggerBuilder</returns>
  IWeeklyTriggerBuild CreateWeeklyTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a monthly execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns>IMonthlyTriggerBuilder</returns>
  IMonthlyTriggerBuilder CreateMonthlyTrigger(DateTimeOffset startDate);
}

