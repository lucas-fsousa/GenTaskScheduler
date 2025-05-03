namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Interface for building a cron trigger.
/// </summary>
public interface ICronTriggerBuilder {
  /// <summary>
  /// Sets the cron expression for the trigger.
  /// </summary>
  /// <param name="expression">Cron expression for execution evaluation</param>
  /// <returns>ICronTriggerBuilder</returns>
  ICronTriggerBuilder SetCronExpression(string expression);
}