using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Models.Triggers;
using System.Threading.Tasks;

namespace GenTaskScheduler.Core.Models.Common;
public class ScheduledTask: BaseModel {
  /// <summary> Task name (used for identification) </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// The next execution time of the task
  /// </summary>
  public DateTimeOffset NextExecution { get; set; }

  /// <summary>
  /// Task state (e.g., running, success, failed, canceled, ready, none)
  /// </summary>
  public ExecutionStatus ExecutionStatus { get; set; } = ExecutionStatus.Ready;

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

  public ExecutionStatus DependsOnStatus { get; set; } = ExecutionStatus.None;
  public Guid? DependsOnTaskId { get; set; }
  public ScheduledTask? DependsOnTask { get; set; }

  public bool AvailableToRun() {
    if(!IsActive || ExecutionStatus == ExecutionStatus.Running || Triggers.Count <= 0)
      return false;

    if(DependsOnTask is null)
      return false;

    var cond1 = DependsOnTask.ExecutionStatus != DependsOnStatus;
    var cond2 = DependsOnTask.ExecutionHistory.Count > 0 && DependsOnTask.ExecutionHistory.Last().Status == DependsOnStatus;
    return cond1 && cond2;
  }
}

