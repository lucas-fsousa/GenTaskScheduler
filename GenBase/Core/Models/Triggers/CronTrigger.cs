using Cronos;

namespace GenTaskScheduler.Core.Models.Triggers;
public class CronTrigger: BaseTrigger {
  /// <summary>
  /// Cron expression that defines the schedule for the task.
  /// </summary>
  public string CronExpression { get; set; } = null!;

  ///<inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    if(string.IsNullOrWhiteSpace(CronExpression))
      return null;

    if(!Cronos.CronExpression.TryParse(CronExpression, out CronExpression cron))
      return null;

    return cron.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
  }

  ///<inheritdoc />
  public override bool IsEligibleToRun() {
    if(string.IsNullOrWhiteSpace(CronExpression))
      return false;

    if(!Cronos.CronExpression.TryParse(CronExpression, out CronExpression cron))
      return false;

    var lastOccurrence = cron.GetNextOccurrence(DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1), TimeZoneInfo.Utc);
    if(!lastOccurrence.HasValue)
      return false;

    return IsWithinMargin(lastOccurrence.Value);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    Executions++;
    UpdatedAt = DateTimeOffset.UtcNow;
    LastExecution = DateTimeOffset.UtcNow;
    NextExecution = GetNextExecution();

    if(MaxExecutions.HasValue && Executions >= MaxExecutions)
      IsValid = false;

    if(EndsAt.HasValue && LastExecution > EndsAt.Value)
      IsValid = false;
  }
}

