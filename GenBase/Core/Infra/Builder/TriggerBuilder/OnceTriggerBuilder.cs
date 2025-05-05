using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class GenSchedulerTriggerBuilder: IOnceTriggerBuilder {

  ///<inheritdoc />
  public IOnceTriggerBuilder CreateOnceTrigger() {
    _current = new OnceTrigger();
    return this;
  }

  ///<inheritdoc />
  ///<exception cref="ArgumentOutOfRangeException"></exception>
  public ICommonTriggerStep SetExecutionDateTime(DateTimeOffset executionTime) {
    if(executionTime < DateTimeOffset.UtcNow)
      throw new ArgumentOutOfRangeException(nameof(executionTime), "Execution time cannot be in the past.");

    _current!.InternalSetStartDate(executionTime);
    _current!.MaxExecutions = 1;
    return this;
  }
}

