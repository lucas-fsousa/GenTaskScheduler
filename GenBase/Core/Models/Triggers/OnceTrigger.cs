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
    var now = DateTimeOffset.UtcNow;
    if(!IsValid || Executions > 0)
      return false;

    if(EndsAt.HasValue && now > EndsAt.Value)
      return false;

    return IsWithinMargin(StartsAt);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    var now = DateTimeOffset.UtcNow;
    LastExecution = now;
    UpdatedAt = now;
    Executions++;
    IsValid = false;
    NextExecution = null;
  }

  /// <inheritdoc />
  public override bool IsMissedTrigger() {
    var now = DateTimeOffset.UtcNow;
    if(!IsValid || MaxExecutions is int max && Executions >= max)
      return false;

    if(NextExecution is not DateTimeOffset expected)
      return false;

    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance;
    return now > expected && now <= expected + tolerance;
  }
}
