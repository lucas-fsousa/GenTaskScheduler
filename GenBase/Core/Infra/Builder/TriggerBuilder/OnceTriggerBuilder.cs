using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class TriggerBuilder: IOnceTriggerBuilder {
  public IOnceTriggerBuilder SetExecutionTime(DateTimeOffset executionTime) {
    if(executionTime < DateTimeOffset.UtcNow)
      throw new ArgumentOutOfRangeException(nameof(executionTime), "Execution time cannot be in the past.");

    _current!.MaxExecutions = 1;
    _current!.StartsAt = executionTime;
    return this;
  }
}

