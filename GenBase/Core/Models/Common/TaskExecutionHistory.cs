using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Models.Common;

/// <summary>
/// Represents the history of a task execution.
/// </summary>
public class TaskExecutionHistory {
  /// <summary>
  /// Unique identifier for the task execution history entry.
  /// </summary>
  public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>
  /// The ID of the scheduled task associated with this execution history.
  /// </summary>
  public Guid TaskId { get; set; }

  /// <summary>The associated scheduled task</summary>
  public ScheduledTask Task { get; set; } = null!;

  /// <summary>The trigger ID that fired this execution (if known)</summary>
  public Guid? TriggerId { get; set; }

  /// <summary>The type of result for the execution (success, failed, canceled, etc.)</summary>
  public string Status { get; set; } = string.Empty;

  /// <summary>UTC time when execution started</summary>
  public DateTimeOffset StartedAt { get; set; }

  /// <summary>UTC time when execution ended</summary>
  public DateTimeOffset? EndedAt { get; set; }

  /// <summary>Optional error message (for failures)</summary>
  public string? ErrorMessage { get; set; }

  /// <summary>
  /// Optional deserialized result data (e.g., output)
  /// </summary>
  public object? ResultObject { get; set; }
}
