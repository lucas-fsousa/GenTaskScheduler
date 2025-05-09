using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

/// <summary>
/// Interface for the final step of building a trigger.
/// </summary>
public interface IFinisheTriggerStep {
  /// <summary>
  /// Builds the trigger and associates it with the task.
  /// </summary>
  void Build();
}