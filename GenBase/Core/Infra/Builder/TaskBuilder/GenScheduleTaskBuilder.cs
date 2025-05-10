using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GenTaskScheduler.Core.Infra.Builder.TaskBuilder;

/// <summary>
/// Builder class for creating scheduled tasks.
/// </summary>
public class GenScheduleTaskBuilder:
    IScheduledTaskBuilderStart,
    IScheduledTaskBuilderJob,
    IScheduledTaskBuilderTriggers,
    IScheduledTaskBuilderOptions,
    IScheduledTaskBuilderDependsOn,
    IScheduledTaskBuilderDependsOnWithStatus {

  private readonly ScheduledTask _task = new();
  private IJob? _job;

  private GenScheduleTaskBuilder(string name) {
    _task.Name = name;
    _task.CreatedAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Creates an instance of the IScheduledTaskBuilderStart implementation with a specified name..
  /// </summary>
  /// <param name="name">The task name to set</param>
  /// <returns> <see cref="IScheduledTaskBuilderStart"/></returns>
  /// <exception cref="ArgumentException"></exception>
  public static IScheduledTaskBuilderStart Create(string name) {
    if(string.IsNullOrEmpty(name))
      throw new ArgumentException("Task name cannot be null or empty", nameof(name));

    return new GenScheduleTaskBuilder(name);
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException"></exception>
  public IScheduledTaskBuilderTriggers WithJob(IJob job) {
    _job = job ?? throw new ArgumentNullException(nameof(job));
    return this;
  }

  /// <inheritdoc />
  public IScheduledTaskBuilderOptions SetAutoDelete(bool value) {
    _task.AutoDelete = value;
    return this;
  }

  /// <inheritdoc />
  public IScheduledTaskBuilderOptions SetIsActive(bool value) {
    _task.IsActive = value;
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentException"></exception>
  public IScheduledTaskBuilderDependsOnWithStatus DependsOn(Guid taskId) {
    if(taskId == Guid.Empty)
      throw new ArgumentException("Task ID cannot be empty", nameof(taskId));

    _task.DependsOnTaskId = taskId;
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException"></exception>
  public IScheduledTaskBuilderDependsOnWithStatus DependsOn(ScheduledTask task) {
    ArgumentNullException.ThrowIfNull(task, nameof(task));
    _task.DependsOnTask = task;
    _task.DependsOnTaskId = task.Id;
    return this;
  }

  /// <inheritdoc />
  public IScheduledTaskBuilderOptions NotDepends() => this;

  /// <inheritdoc />
  public IScheduledTaskBuilderOptions WithStatus(GenTaskHistoryStatus status) {
    _task.DependsOnStatus = status.ToString();
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentException"></exception>
  public IScheduledTaskBuilderOptions WithStatus(params GenTaskHistoryStatus[] status) {
    if(status.Length <= 0)
      throw new ArgumentException("Status array cannot be empty", nameof(status));

    if(status.Any(s => s == GenTaskHistoryStatus.None))
      throw new ArgumentException("Status cannot be None", nameof(status));

    _task.DependsOnStatus = string.Join(',', status.Select(x => x.ToString()));
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="InvalidOperationException"></exception>
  public ScheduledTask Build() {
    if(_job == null)
      throw new InvalidOperationException("A job must be defined using WithJob()");

    if(_task.Triggers.Count == 0)
      throw new InvalidOperationException($"At least one trigger must be defined. Use {nameof(IScheduledTaskBuilderTriggers.ConfigureTriggers)} method");

    _task.BlobArgs = TaskSerializer.Serialize(_job);
    return _task;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException"></exception>
  public IScheduledTaskBuilderDependsOn ConfigureTriggers(Action<ITriggerBuilderStart> configure) {
    ArgumentNullException.ThrowIfNull(configure);
    configure(GenSchedulerTriggerBuilder.Start(_task));
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException"></exception>
  public IScheduledTaskBuilderDependsOn AddTrigger(BaseTrigger trigger) {
    ArgumentNullException.ThrowIfNull(trigger);
    AddTriggers(trigger);
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="ArgumentException"></exception>
  public IScheduledTaskBuilderDependsOn AddTriggers(params BaseTrigger[] triggers) {
    ArgumentNullException.ThrowIfNull(triggers);

    if(triggers.Length == 0)
      throw new ArgumentException("The array of triggers cannot be empty", nameof(triggers));

    foreach(var trigger in triggers) {
      trigger.TaskId = _task.Id;
      trigger.Task = _task;
      _task.Triggers.Add(trigger);
    }

    return this;
  }

  /// <inheritdoc />
  /// <exception cref="ArgumentException"></exception>
  public IScheduledTaskBuilderOptions SetTimeout(TimeSpan timeout) {
    if(timeout == TimeSpan.Zero)
      throw new ArgumentException("Timeout cannot be zero", nameof(timeout));

    _task.MaxExecutionTime = timeout;
    return this;
  }
}
