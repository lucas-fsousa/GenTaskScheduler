namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

/// <summary>
/// Interface for common trigger steps.
/// </summary>
public interface ICommonTriggerStep : IFinisheTriggerStep {

  /// <summary>
  /// Sets the description for the trigger.
  /// </summary>
  /// <param name="description">Value to set</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetDescription(string description);

  /// <summary>
  /// Sets the validity of the trigger.
  /// </summary>
  /// <param name="endsAt">Value indicating the end of the trigger (if null, it indicates that the trigger has no expiration)</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetValidity(DateTimeOffset? endsAt = null);

  /// <summary>
  /// Sets the auto-delete property of the trigger.
  /// </summary>
  /// <param name="autoDelete">value to set</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetAutoDelete(bool autoDelete);

  /// <summary>
  /// Sets the maximum number of executions for the trigger.
  /// </summary>
  /// <param name="maxExecutions">Value indicating the maximum number of executions that the trigger should execute (if null, there are no execution limits)</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetExecutionLimit(int? maxExecutions);
}
