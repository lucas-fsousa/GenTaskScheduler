using GenTaskScheduler.Core.Infra.Configurations;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs only once at a specified execution time.
/// </summary>
public class OnceTrigger: BaseTrigger {

  /// <inheritdoc/>
  public override DateTimeOffset? GetNextExecution() => Executions == 0 ? StartsAt : null;

  /// <inheritdoc/>
  public override bool IsEligibleToRun() {
    if(!IsValid || Executions > 0)
      return false;

    if(EndsAt.HasValue && DateTimeOffset.UtcNow > EndsAt.Value)
      return false;

    return IsWithinMargin(StartsAt);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    LastExecution = DateTimeOffset.UtcNow;
    UpdatedAt = DateTimeOffset.UtcNow;
    Executions++;
    IsValid = false;
    NextExecution = null;
  }

  /// <inheritdoc />
  public override bool IsMissedTrigger() {
    if(!IsValid || MaxExecutions is int max && Executions >= max)
      return false;

    if(NextExecution is not DateTimeOffset expected)
      return false;

    var now = DateTimeOffset.UtcNow;
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance;
    return now > expected && now <= expected + tolerance;
  }
}
