using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Interface for building a once trigger.
/// </summary>
public interface IOnceTriggerBuilder {
  ICommonTriggerStep SetExecutionDateTime(DateTimeOffset executionTime);
}