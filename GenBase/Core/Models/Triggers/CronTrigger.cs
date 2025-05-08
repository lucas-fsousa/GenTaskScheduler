using Cronos;

namespace GenTaskScheduler.Core.Models.Triggers;
public class CronTrigger: BaseTrigger {
  /// <summary>
  /// Cron expression that defines the schedule for the task.
  /// </summary>
  public string CronExpression { get; set; } = null!;

  ///<inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    var now = DateTimeOffset.UtcNow;
    if(string.IsNullOrWhiteSpace(CronExpression))
      return null;

    if(!Cronos.CronExpression.TryParse(CronExpression, out CronExpression cron))
      return null;

    return cron.GetNextOccurrence(now, TimeZoneInfo.Utc);
  }

  ///<inheritdoc />
  public override bool IsEligibleToRun() {
    var now = DateTimeOffset.UtcNow;
    if(string.IsNullOrWhiteSpace(CronExpression))
      return false;

    if(!Cronos.CronExpression.TryParse(CronExpression, out CronExpression cron))
      return false;

    var lastOccurrence = cron.GetNextOccurrence(now - TimeSpan.FromMinutes(1), TimeZoneInfo.Utc);
    if(!lastOccurrence.HasValue)
      return false;

    return IsWithinMargin(lastOccurrence.Value);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    var now = DateTimeOffset.UtcNow;
    Executions++;
    UpdatedAt = now;
    LastExecution = now;
    NextExecution = GetNextExecution();

    if(MaxExecutions.HasValue && Executions >= MaxExecutions)
      IsValid = false;

    if(EndsAt.HasValue && LastExecution > EndsAt.Value)
      IsValid = false;
  }
}

