using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Models.Common;

/// <summary>
/// Represents a scheduled task in the system.
/// </summary>
public class ScheduledTask: BaseModel {
  /// <summary> Task name (used for identification) </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// The next execution time of the task
  /// </summary>
  public DateTimeOffset NextExecution { get; set; }

  /// <summary>
  /// The last execution time of the task 
  /// </summary>
  public DateTimeOffset LastExecution { get; set; } = DateTimeOffset.MinValue;

  /// <summary>
  /// Task state (e.g., running, waiting, ready)
  /// </summary>
  public string ExecutionStatus { get; set; } = GenSchedulerTaskStatus.Ready.ToString();

  /// <summary> If true, the task will be deleted automatically after completion </summary>
  public bool AutoDelete { get; set; }

  /// <summary> If false, the task is ignored/skipped by the scheduler </summary>
  public bool IsActive { get; set; } = true;

  /// <summary> Arbitrary serialized arguments to be passed to the job handler </summary>
  public byte[] BlobArgs { get; set; } = [];

  /// <summary>
  /// Maximum time a task can remain running. 
  /// After reaching the limit, all tasks linked to it will automatically be interrupted (timeout).
  /// If this value is <see cref="TimeSpan.Zero"/> has no limit.
  /// </summary>
  public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.Zero;

  /// <summary> Triggers associated with this task </summary>
  public ICollection<BaseTrigger> Triggers { get; set; } = [];

  /// <summary>Execution logs associated with this task</summary>
  public ICollection<TaskExecutionHistory> ExecutionHistory { get; set; } = [];

  /// <summary>
  /// Task history status that this task depends on.
  /// </summary>
  public string DependsOnStatus { get; set; } = string.Empty;

  /// <summary>
  /// Task ID that this task depends on.
  /// </summary>
  public Guid? DependsOnTaskId { get; set; }

  /// <summary>
  /// Task that this task depends on.
  /// </summary>
  public ScheduledTask? DependsOnTask { get; set; }

  /// <summary>
  /// Checks if the task is available to run based on its status and dependencies.
  /// </summary>
  /// <returns>returns true if the evaluation criteria are met.</returns>
  public bool AvailableToRun() {
    if(!IsActive || ExecutionStatus == GenSchedulerTaskStatus.Running.ToString() || Triggers.Count <= 0)
      return false;

    if(DependsOnTask is null)
      return true;

    return DependsOnTask.ExecutionHistory.Count > 0 && DependsOnStatus.Split(',').Contains(DependsOnTask.ExecutionHistory.Last().Status);
  }
}

