using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

public partial class GenSchedulerTriggerBuilder:
  ICommonTriggerStep,
  ITriggerBuilderStart {
  internal readonly ScheduledTask _task;
  internal BaseTrigger? _current;
  private GenSchedulerTriggerBuilder(ScheduledTask task) {
    _task = task;
  }

  public static ITriggerBuilderStart Start(ScheduledTask task) => new GenSchedulerTriggerBuilder(task);

  public ICommonTriggerStep SetValidity(DateTimeOffset? endsAt = null) {
    _current!.InternalSetValidity(endsAt);
    return this;
  }

  public ICommonTriggerStep SetAutoDelete(bool autoDelete) {
    _current!.InternalSetAutoDelete(autoDelete);
    return this;
  }

  public ICommonTriggerStep SetDescription(string description) {
    _current!.InternalSetDescription(description);
    return this;
  }

  public ICommonTriggerStep SetExecutionLimit(int? maxExecutions) {
    _current!.InternalSetMaxExecutionLimit(maxExecutions);
    return this;
  }



  public void Build() {
    if(_current == null)
      throw new InvalidOperationException("No trigger has been created. Use the appropriate method to create a trigger before building.");

    _current.TaskId = _task.Id;
    _current.Task = _task;
    _task.Triggers.Add(_current);
    _current = null;
  }
}
