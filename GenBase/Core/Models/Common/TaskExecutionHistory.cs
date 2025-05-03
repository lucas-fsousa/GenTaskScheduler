using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Models.Common;

public class TaskExecutionHistory {
  public Guid Id { get; set; } = Guid.NewGuid();

  public Guid TaskId { get; set; }

  /// <summary>The associated scheduled task</summary>
  public ScheduledTask Task { get; set; } = null!;

  /// <summary>The trigger ID that fired this execution (if known)</summary>
  public Guid? TriggerId { get; set; }

  /// <summary>The type of result for the execution (success, failed, skipped, etc.)</summary>
  public ExecutionStatus Status { get; set; }

  /// <summary>UTC time when execution started</summary>
  public DateTimeOffset StartedAt { get; set; }

  /// <summary>UTC time when execution ended</summary>
  public DateTimeOffset? EndedAt { get; set; }

  /// <summary>Optional error message (for failures)</summary>
  public string? ErrorMessage { get; set; }

  /// <summary>Optional serialized result data (e.g., output)</summary>
  public byte[]? ResultBlob { get; set; }

  /// <summary>
  /// Optional deserialized result data (e.g., output)
  /// </summary>
  public object? ResultObject { get; set; }
}
