namespace GenTaskScheduler.Core.Models.Common;

/// <summary>
/// Base model for all entities.
/// </summary>
public abstract class BaseModel {
  /// <summary>
  /// Unique identifier for the entity.
  /// </summary>
  public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>
  /// The date and time when the entity was created.
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  /// <summary>
  /// The date and time when the entity was last updated.
  /// </summary>
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

