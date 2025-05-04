using GenTaskScheduler.Core.Infra.Configurations;

namespace GenTaskScheduler.Core.Models.Triggers;
public class CronTrigger: BaseTrigger {
  /// <summary>
  /// Cron expression that defines the schedule for the task.
  /// </summary>
  public string CronExpression { get; set; } = null!;

  public override DateTimeOffset? GetNextExecution() {
    if(string.IsNullOrWhiteSpace(CronExpression))
      return null;

    var cron = Cronos.CronExpression.Parse(CronExpression);
    return cron.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
  }

  public override bool IsEligibleToRun() {
    if(string.IsNullOrWhiteSpace(CronExpression))
      return false;

    var cron = Cronos.CronExpression.Parse(CronExpression);
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

