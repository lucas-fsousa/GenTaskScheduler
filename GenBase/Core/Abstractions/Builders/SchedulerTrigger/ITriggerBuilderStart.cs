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
  /// <returns><see cref="IOnceTriggerBuilder"/></returns>
  IOnceTriggerBuilder CreateOnceTrigger();

  /// <summary>
  /// Creates a trigger builder for an interval-based execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns><see cref="IIntervalTriggerBuilder"/></returns>
  IIntervalTriggerBuilder CreateIntervalTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a cron-based execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns><see cref="ICronExpressionTriggerStep"/></returns>
  ICronExpressionTriggerStep CreateCronTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a calendar-based execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns><see cref="ICalendarTriggerBuilder"/></returns>
  ICalendarTriggerBuilder CreateCalendarTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a daily execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns><see cref="IDailyTriggerBuilder"/></returns>
  IDailyTriggerBuilder CreateDailyTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a weekly execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns><see cref="IWeeklyTriggerBuild"/></returns>
  IWeeklyTriggerBuild CreateWeeklyTrigger(DateTimeOffset startDate);

  /// <summary>
  /// Creates a trigger builder for a monthly execution trigger.
  /// </summary>
  /// <param name="startDate">Initial start date</param>
  /// <returns><see cref="IMonthlyTriggerBuilder"/></returns>
  IMonthlyTriggerBuilder CreateMonthlyTrigger(DateTimeOffset startDate);
}

