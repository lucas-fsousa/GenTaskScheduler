using Cronos;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Helper;
public static class TriggerEvaluator {
  /// <summary>
  /// Checks if an IntervalTrigger should be executed based on the current time and configuration.
  /// </summary>
  public static Guid? ShouldExecuteIntervalTrigger(IntervalTrigger trigger, DateTimeOffset utcNow, SchedulerConfiguration config) {
    if(!trigger.IsValid || utcNow < trigger.StartsAt)
      return null;

    if(trigger.EndsAt is not null && utcNow > trigger.EndsAt.Value)
      return null;

    if(trigger.MaxExecutions is not null && trigger.Executions >= trigger.MaxExecutions)
      return null;

    var timeSinceStart = utcNow - trigger.InitialExecutionTime;
    if(timeSinceStart.TotalMinutes < 0)
      return null;

    var intervalsPassed = (int)(timeSinceStart.TotalMinutes / trigger.RepeatIntervalMinutes);
    var expectedTime = trigger.InitialExecutionTime.AddMinutes(intervalsPassed * trigger.RepeatIntervalMinutes);

    if(utcNow < expectedTime)
      return null;

    if(utcNow > expectedTime.Add(config.MarginOfError))
      return null;

    if(trigger.LastExecution.HasValue && trigger.LastExecution.Value >= expectedTime)
      return null;

    return trigger.Id;
  }

  /// <summary>
  /// Checks if an OnceTrigger should be executed based on the current time and configuration.
  /// </summary>
  public static Guid? ShouldExecuteOnceTrigger(OnceTrigger trigger, DateTimeOffset utcNow, SchedulerConfiguration config) {
    if(!trigger.IsValid || trigger.Executed)
      return null;

    var scheduledTime = trigger.StartsAt;
    if(utcNow < scheduledTime)
      return null;

    if(utcNow > scheduledTime.Add(config.MarginOfError))
      return null;

    if(trigger.LastExecution.HasValue && trigger.LastExecution.Value >= scheduledTime)
      return null;

    return trigger.Id;
  }

  /// <summary>
  /// Checks if a CronTrigger should be executed based on the current time and configuration.
  /// </summary>
  public static Guid? ShouldExecuteCronTrigger(CronTrigger trigger, DateTimeOffset utcNow, SchedulerConfiguration config) {
    if(!trigger.IsValid || utcNow < trigger.StartsAt)
      return null;

    if(trigger.EndsAt is not null && utcNow > trigger.EndsAt.Value)
      return null;

    if(trigger.MaxExecutions is not null && trigger.Executions >= trigger.MaxExecutions)
      return null;

    var expression = CronExpression.Parse(trigger.CronExpression);
    var lastExpected = expression.GetNextOccurrence(utcNow.AddMinutes(-1), TimeZoneInfo.Utc);
    if(lastExpected is null)
      return null;

    var expectedTime = lastExpected.Value;

    if(utcNow < expectedTime)
      return null;

    if(utcNow > expectedTime.Add(config.MarginOfError))
      return null;

    if(trigger.LastExecution.HasValue && trigger.LastExecution.Value >= expectedTime)
      return null;

    return trigger.Id;
  }

  /// <summary>
  /// Checks if a DayWeekMonthTrigger should be executed based on the current time and configuration.
  /// </summary>
  public static Guid? ShouldExecuteDayWeekMonthTrigger(MonthlyTrigger trigger, DateTimeOffset utcNow, SchedulerConfiguration config) {
    if(!trigger.IsValid)
      return null;

    if(!trigger.MonthsOfYear.Split(',').Select(int.Parse).Contains(utcNow.Month))
      return null;

    if(!trigger.DaysOfMonth.Split(',').Select(int.Parse).Contains(utcNow.Day))
      return null;

    var scheduledTime = utcNow.Date.Add(trigger.TimeOfDay);
    if(utcNow < scheduledTime)
      return null;

    if(utcNow > scheduledTime.Add(config.MarginOfError))
      return null;

    if(trigger.LastExecution.HasValue && trigger.LastExecution.Value >= scheduledTime)
      return null;

    return trigger.Id;
  }

  /// <summary>
  /// Check if the task should be executed based on the defined calendar entries and the current time.
  /// </summary>
  /// <param name="utcNow">The current UTC time.</param>
  /// <param name="marginOfError">The allowed margin of error for execution.</param>
  /// <returns>True if the task should execute, false otherwise.</returns>
  public static Guid? ShouldExecuteCalendarTrigger(CalendarTrigger trigger, DateTimeOffset utcNow, SchedulerConfiguration config) {
    if(!trigger.IsValid)
      return null;

    foreach(var entry in trigger.CalendarEntries) {
      var scheduledTime = entry.ScheduledDateTime;

      if(utcNow < scheduledTime)
        continue;

      if(utcNow > scheduledTime.Add(config.MarginOfError))
        continue;

      if(trigger.LastExecution.HasValue && trigger.LastExecution.Value >= scheduledTime)
        continue;

      return entry.Id;
    }

    return null;
  }
}


