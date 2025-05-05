using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Base class for all triggers in the task scheduler.
/// </summary>
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
  /// The last status of the trigger after it was executed
  /// </summary>
  public string LastTriggeredStatus { get; set; } = GenTriggerTriggeredStatus.NotTriggered.ToString();

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

  /// <summary>
  /// The specific time of day when the task should be executed.
  /// </summary>
  public TimeOnly TimeOfDay { get; set; }

  /// <summary>
  /// Calculates the next execution time based on the current time and the trigger's settings.
  /// </summary>
  /// <returns> The <see cref="DateTimeOffset"/> of the next run. </returns>
  public abstract DateTimeOffset? GetNextExecution();

  /// <summary>
  /// Checks if the trigger can be fired based on its settings.
  /// </summary>
  /// <returns>Returns true if trigger criteria are met.</returns>
  public abstract bool IsEligibleToRun();

  /// <summary>
  /// Updates the trigger's state (validity, execution count, last execution, etc.) based on its type and conditions.
  /// This method should be called by the Launcher after the trigger is executed.
  /// </summary>
  public abstract void UpdateTriggerState();

  /// <summary>
  /// Checks if the current time is within the margin of error for late execution.
  /// </summary>
  /// <param name="expectedExecution">Date/time when execution should happen</param>
  /// <returns>Returns true if it is within the time window allowed for execution</returns>
  protected static bool IsWithinMargin(DateTimeOffset expectedExecution) {
    var now = DateTimeOffset.UtcNow;
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance;
    return Math.Abs((now - expectedExecution).TotalSeconds) <= tolerance.TotalSeconds;
  }

  /// <summary>
  /// Determines if the trigger has missed its expected execution but is still within the allowed tolerance window.
  /// </summary>
  /// <returns>True if the trigger missed execution within tolerance; otherwise, false.</returns>
  public virtual bool IsMissedTrigger() {
    if(!IsValid || MaxExecutions is int max && Executions >= max)
      return false;

    var expected = GetNextExecution();
    if(!expected.HasValue)
      return false;

    var now = DateTimeOffset.UtcNow;
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance;

    return now > expected.Value + tolerance;
  }

}
