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
  /// Task state (e.g., running, waiting, ready)
  /// </summary>
  public GenSchedulerTaskStatus ExecutionStatus { get; set; } = GenSchedulerTaskStatus.Ready;

  /// <summary> If true, the task will be deleted automatically after completion </summary>
  public bool AutoDelete { get; set; }

  /// <summary> If false, the task is ignored/skipped by the scheduler </summary>
  public bool IsActive { get; set; } = true;

  /// <summary> Arbitrary serialized arguments to be passed to the job handler </summary>
  public byte[] BlobArgs { get; set; } = [];

  /// <summary> Triggers associated with this task </summary>
  public ICollection<BaseTrigger> Triggers { get; set; } = [];

  /// <summary>Execution logs associated with this task</summary>
  public ICollection<TaskExecutionHistory> ExecutionHistory { get; set; } = [];

  /// <summary>
  /// Task history status that this task depends on.
  /// </summary>
  public GenTaskHistoryStatus DependsOnStatus { get; set; } = GenTaskHistoryStatus.None;

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
    if(!IsActive || ExecutionStatus == GenSchedulerTaskStatus.Running || Triggers.Count <= 0)
      return false;

    if(DependsOnTask is null)
      return true;

    return DependsOnTask.ExecutionHistory.Count > 0 && DependsOnTask.ExecutionHistory.Last().Status == DependsOnStatus;
  }
}

