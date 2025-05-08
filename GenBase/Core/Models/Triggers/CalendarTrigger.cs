using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs on specific dates and times.
/// </summary>
public class CalendarTrigger: BaseTrigger {
  /// <summary>
  /// List of specific date/times when the task should run.
  /// </summary>
  public List<CalendarEntry> CalendarEntries { get; set; } = [];

  /// <inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    var now = DateTimeOffset.UtcNow;
    return CalendarEntries.FirstOrDefault(entry => !entry.Executed && entry.ScheduledDateTime > now)?.ScheduledDateTime;
  }

  /// <inheritdoc />
  public override bool IsEligibleToRun() {
    var now = DateTimeOffset.UtcNow;
    if(!IsValid || (EndsAt.HasValue && now > EndsAt.Value))
      return false;

    return CalendarEntries.Any(entry => !entry.Executed && entry.ScheduledDateTime <= now && IsWithinMargin(entry.ScheduledDateTime));
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    var now = DateTimeOffset.UtcNow;
    Executions++;
    UpdatedAt = now;
    LastExecution = now;
    NextExecution = GetNextExecution();

    if(NextExecution is null || (MaxExecutions.HasValue && Executions >= MaxExecutions)) {
      IsValid = false;
      NextExecution = null;
      return;
    }

    var justExecuted = CalendarEntries
      .Where(e => !e.Executed && e.ScheduledDateTime <= LastExecution)
      .OrderByDescending(e => e.ScheduledDateTime)
      .FirstOrDefault();

    if(justExecuted is not null)
      justExecuted.Executed = true;
  }

  /// <inheritdoc />
  public override bool IsMissedTrigger() {
    var now = DateTimeOffset.UtcNow;
    if(!IsValid || MaxExecutions is int max && Executions >= max)
      return false;

    var next = CalendarEntries
      .Where(e => !e.Executed && e.ScheduledDateTime > LastExecution)
      .OrderBy(e => e.ScheduledDateTime)
      .FirstOrDefault();

    if(next == null)
      return false;

    var expected = next.ScheduledDateTime;
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance;
    return now > expected && now <= expected + tolerance;
  }

}

