using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs only once at a specified execution time.
/// </summary>
public class OnceTrigger: BaseTrigger {
  /// <summary>
  /// Indicates if the trigger has already been executed.
  /// </summary>
  public bool Executed { get; set; }
}
