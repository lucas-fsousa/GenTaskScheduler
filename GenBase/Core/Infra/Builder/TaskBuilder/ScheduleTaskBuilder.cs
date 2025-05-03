using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TaskBuilder;
public class ScheduleTaskBuilder:
    IScheduledTaskBuilderStart,
    IScheduledTaskBuilderJob,
    IScheduledTaskBuilderTriggers,
    IScheduledTaskBuilderOptions,
    IScheduledTaskBuilderDependsOn,
    IScheduledTaskBuilderDependsOnWithStatus {

  private readonly ScheduledTask _task = new();
  private IJob? _job;

  private ScheduleTaskBuilder(string name) {
    _task.Name = name;
    _task.CreatedAt = DateTimeOffset.UtcNow;
  }

  public static IScheduledTaskBuilderStart Create(string name) {
    if(string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Task name cannot be null or empty", nameof(name));

    return new ScheduleTaskBuilder(name);
  }

  public IScheduledTaskBuilderTriggers WithJob(IJob job) {
    _job = job ?? throw new ArgumentNullException(nameof(job));
    return this;
  }

  public IScheduledTaskBuilderDependsOn AddTrigger(BaseTrigger trigger) {
    _task.Triggers.Add(trigger);
    return this;
  }

  public IScheduledTaskBuilderDependsOn AddTriggers(IEnumerable<BaseTrigger> triggers) {
    foreach(var trigger in triggers) {
      _task.Triggers.Add(trigger);
    }
    return this;
  }

  public IScheduledTaskBuilderDependsOn AddTrigger<T>(Action<T> configure) where T : BaseTrigger {
    var trigger = Activator.CreateInstance(typeof(T)) as T ??
      throw new InvalidOperationException($"Failed to instantiate trigger of type '{typeof(T).Name}'. Ensure it has a parameterless constructor and is not abstract.");
    configure?.Invoke(trigger);
    _task.Triggers.Add(trigger);
    return this;
  }

  public IScheduledTaskBuilderOptions SetAutoDelete(bool value) {
    _task.AutoDelete = value;
    return this;
  }

  public IScheduledTaskBuilderOptions SetIsActive(bool value) {
    _task.IsActive = value;
    return this;
  }

  public IScheduledTaskBuilderTriggers Done() => this;


  public IScheduledTaskBuilderDependsOnWithStatus DependsOn(Guid taskId) {
    if(taskId == Guid.Empty)
      throw new ArgumentException("Task ID cannot be empty", nameof(taskId));

    _task.DependsOnTaskId = taskId;
    return this;
  }

  public IScheduledTaskBuilderDependsOnWithStatus DependsOn(ScheduledTask task) {
    ArgumentNullException.ThrowIfNull(task, nameof(task));
    _task.DependsOnTask = task;
    _task.DependsOnTaskId = task.Id;
    return this;
  }

  public IScheduledTaskBuilderOptions NotDepends() => this;

  public IScheduledTaskBuilderOptions WithStatus(ExecutionStatus status) {
    if(status == ExecutionStatus.None)
      throw new ArgumentException("Status cannot be None", nameof(status));

    _task.DependsOnStatus = status;
    return this;
  }

  public ScheduledTask Build() {
    if(_job == null)
      throw new InvalidOperationException("A job must be defined using WithJob()");

    if(_task.Triggers.Count == 0)
      throw new InvalidOperationException("At least one trigger must be defined");

    _task.BlobArgs = TaskSerializer.Serialize(_job);

    foreach(var trigger in _task.Triggers) {
      trigger.TaskId = _task.Id;
      if(trigger is OnceTrigger)
        trigger.MaxExecutions = 1;
    }

    return _task;
  }

  public IScheduledTaskBuilderDependsOn ConfigureTriggers(Action<ITriggerBuilderStart> configure) {
    ArgumentNullException.ThrowIfNull(configure);
    var builder = TriggerBuilder.TriggerBuilder.Start();
    configure(builder);

    return this;
  }
}
