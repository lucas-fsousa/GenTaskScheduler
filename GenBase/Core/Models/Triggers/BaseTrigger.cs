using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;
public abstract class BaseTrigger : BaseModel {
  /// <summary>
  /// ID of the task associated with this trigger
  /// </summary>
  public Guid TaskId { get; set; }

  /// <summary>
  /// Task associated with this trigger
  /// </summary>
  public ScheduledTask Task { get; set; } = null!;

  /// <summary>
  /// Date and time when the trigger becomes active
  /// </summary>
  public DateTimeOffset StartsAt { get; set; }

  /// <summary>
  /// Optional expiration date and time. If null, the trigger does not expire
  /// </summary>
  public DateTimeOffset? EndsAt { get; set; }

  /// <summary>
  /// If true, the trigger will be automatically deleted after its validity ends
  /// </summary>
  public bool ShouldAutoDelete { get; set; }

  /// <summary>
  /// Optional interval between executions, if the trigger supports repetition
  /// </summary>
  public TimeSpan? ExecutionInterval { get; set; }

  /// <summary>
  /// Description of the trigger, used for logging, debuggin or user interface purposes
  /// </summary>
  public string TriggerDescription { get; set; } = string.Empty;

  /// <summary>
  /// Indicates if the trigger is valid and can be executed
  /// </summary>
  public bool IsValid { get; set; } = true;

  /// <summary>
  /// Represents the last execution time of the trigger
  /// </summary>
  public DateTimeOffset? LastExecution { get; set; }

  /// <summary>
  /// Optional next execution time of the trigger
  /// </summary>
  public DateTimeOffset? NextExecution { get; set; }

  /// <summary>
  /// Optional maximum number of executions
  /// </summary>
  public int? MaxExecutions { get; set; }

  /// <summary>
  /// Total number of times there have been executions so far.
  /// </summary>
  public int Executions { get; set; }
}
