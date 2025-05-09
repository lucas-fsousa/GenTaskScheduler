namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Trigger for daily execution at a specific time
/// </summary>
public class DailyTrigger: BaseTrigger {

  /// <inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    var now = DateTimeOffset.UtcNow;

    if(!IsValid || (EndsAt.HasValue && now > EndsAt))
      return null;

    var todayExecution = now.Date + TimeOfDay.ToTimeSpan();

    if(todayExecution < StartsAt)
      todayExecution = StartsAt.UtcDateTime;

    if(todayExecution >= now)
      return todayExecution;

    var next = todayExecution.AddDays(1);
    return (EndsAt.HasValue && next > EndsAt) ? null : next;
  }


  /// <inheritdoc />
  public override bool IsEligibleToRun() {
    if(!IsValid || MaxExecutions.HasValue && Executions >= MaxExecutions)
      return false;

    var next = GetNextExecution();
    if(!next.HasValue)
      return false;

    return IsWithinMargin(next.Value);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    var now = DateTimeOffset.UtcNow;
    Executions++;
    UpdatedAt = now;
    LastExecution = now;
    NextExecution = GetNextExecution();

    if(NextExecution is null) {
      IsValid = false;
      return;
    }

    if(EndsAt.HasValue && NextExecution > EndsAt.Value) {
      IsValid = false;
      NextExecution = null;
      return;
    }

    if(MaxExecutions.HasValue && Executions >= MaxExecutions) {
      IsValid = false;
      NextExecution = null;
      return;
    }
  }

}

