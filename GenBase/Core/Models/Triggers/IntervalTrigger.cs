namespace GenTaskScheduler.Core.Models.Triggers;
/// <summary>
/// A trigger that fires repeatedly at a fixed interval starting from a given start time.
/// </summary>
public class IntervalTrigger: BaseTrigger {
  
  /// <inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    if(!IsValid || ExecutionInterval is null || ExecutionInterval.Value.TotalMinutes <= 0)
      return null;

    var now = DateTimeOffset.UtcNow;

    if(now < StartsAt)
      return StartsAt;

    if(EndsAt.HasValue && now > EndsAt.Value)
      return null;

    var elapsed = now - StartsAt;
    var intervalsPassed = (int)Math.Floor(elapsed.TotalMinutes / ExecutionInterval.Value.TotalMinutes);
    var next = StartsAt.AddMinutes((intervalsPassed + 1) * ExecutionInterval.Value.TotalMinutes);

    if(EndsAt.HasValue && next > EndsAt.Value)
      return null;

    return next;
  }

  /// <inheritdoc />
  public override bool IsEligibleToRun() {
    if(!IsValid)
      return false;

    if(MaxExecutions.HasValue && Executions >= MaxExecutions)
      return false;

    var next = GetNextExecution();
    return next.HasValue && IsWithinMargin(next.Value);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    Executions++;
    UpdatedAt = DateTimeOffset.UtcNow;
    LastExecution = DateTimeOffset.UtcNow;
    NextExecution = GetNextExecution();

    if(MaxExecutions.HasValue && Executions >= MaxExecutions)
      IsValid = false;

    if(EndsAt.HasValue && NextExecution > EndsAt.Value)
      IsValid = false;
  }
}
