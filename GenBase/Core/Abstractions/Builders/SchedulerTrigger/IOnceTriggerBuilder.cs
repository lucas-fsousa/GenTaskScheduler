namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

/// <summary>
/// Interface for building a once trigger.
/// </summary>
public interface IOnceTriggerBuilder {
  IOnceTriggerBuilder SetExecutionTime(DateTimeOffset executionTime);
}