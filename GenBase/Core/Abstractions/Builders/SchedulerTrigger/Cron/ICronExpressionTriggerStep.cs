using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Cron;

/// <summary>
/// Interface for building a cron trigger.
/// </summary>
public interface ICronExpressionTriggerStep {
  /// <summary>
  /// Sets the cron expression for the trigger.
  /// </summary>
  /// <param name="expression">Cron expression for execution evaluation</param>
  /// <returns>ITriggerCommonStep</returns>
  ICommonTriggerStep SetCronExpression(string expression);
}