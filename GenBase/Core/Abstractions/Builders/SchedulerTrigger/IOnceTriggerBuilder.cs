using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Interface for building a once trigger.
/// </summary>
public interface IOnceTriggerBuilder {
  /// <summary>
  /// Creates a new instance of a once trigger.
  /// </summary>
  /// <param name="executionTime">Value to define the date/time that the trigger should be executed.</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  ICommonTriggerStep SetExecutionDateTime(DateTimeOffset executionTime);

  /// <summary>
  /// Creates a new instance of a once trigger.
  /// </summary>
  /// <param name="executionTime">Value to define the date/time that the trigger should be executed.</param>
  /// <returns><see cref="ICommonTriggerStep"/></returns>
  [Obsolete("Use SetExecutionDateTime(DateTimeOffset) instead.")]
  ICommonTriggerStep SetExecutionDateTime(DateTime executionTime);
}