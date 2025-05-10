namespace GenTaskScheduler.Core.Models.Triggers;
/// <summary>
/// A trigger that fires repeatedly at a fixed interval starting from a given start time.
/// </summary>
public class IntervalTrigger: BaseTrigger {

  /// <inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    var now = DateTimeOffset.UtcNow;
    if(!IsValid || ExecutionInterval is null || ExecutionInterval.Value.TotalMinutes <= 0)
      return null;

    if(now < StartsAt)
      return StartsAt;

    if(EndsAt.HasValue && now > EndsAt.Value)
      return null;

    var baseTime = LastExecution ?? StartsAt;
    var next = baseTime.Add(ExecutionInterval.Value);

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
    LastExecution = DateTimeOffset.UtcNow;
    UpdatedAt = LastExecution.Value;
    NextExecution = GetNextExecution();

    if(MaxExecutions.HasValue && Executions >= MaxExecutions) {
      IsValid = false;
      Executions = MaxExecutions.Value;
    }
      

    if(EndsAt.HasValue && NextExecution.HasValue && NextExecution > EndsAt.Value)
      IsValid = false;
  }

}
