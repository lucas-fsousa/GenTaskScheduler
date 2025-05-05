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

    return CalendarEntries
      .Where(entry => !entry.Executed && entry.ScheduledDateTime > now)
      .OrderBy(entry => entry.ScheduledDateTime)
      .FirstOrDefault()?.ScheduledDateTime;
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
    Executions++;
    UpdatedAt = DateTimeOffset.UtcNow;
    LastExecution = DateTimeOffset.UtcNow;
    NextExecution = GetNextExecution();

    if(NextExecution is null) {
      IsValid = false;
      return;
    }

    if(NextExecution == default || (MaxExecutions.HasValue && Executions >= MaxExecutions)) {
      IsValid = false;
      NextExecution = null;
      return;
    }

    var entry = CalendarEntries.LastOrDefault(CalendarEntries => CalendarEntries.ScheduledDateTime < NextExecution);
    if(entry is not null)
      CalendarEntries.First(x => x.Id == entry.Id).Executed = true;
  }

  /// <inheritdoc />
  public override bool IsMissedTrigger() {
    if(!IsValid || MaxExecutions is int max && Executions >= max)
      return false;

    var next = CalendarEntries.OrderBy(e => e.ScheduledDateTime).FirstOrDefault(e => e.ScheduledDateTime > LastExecution);
    if(next == null)
      return false;

    var now = DateTimeOffset.UtcNow;
    var expected = next.ScheduledDateTime;
    var tolerance = GenSchedulerEnvironment.SchedulerConfiguration.LateExecutionTolerance;

    return now > expected && now <= expected + tolerance;
  }

}

