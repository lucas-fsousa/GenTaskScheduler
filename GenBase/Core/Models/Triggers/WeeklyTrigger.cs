namespace GenTaskScheduler.Core.Models.Triggers;
/// <summary>
/// Trigger for weekly execution on specific days and times.
/// </summary>
public class WeeklyTrigger: BaseTrigger {
  /// <summary>
  /// The days of the week on which the task should run. Represented as a comma-separated list (e.g., "Monday,Wednesday").
  /// </summary>
  public string DaysOfWeek { get; set; } = string.Empty;

  /// <inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    if(!IsValid || string.IsNullOrWhiteSpace(DaysOfWeek))
      return null;

    var now = DateTimeOffset.UtcNow;
    if(EndsAt.HasValue && now > EndsAt.Value)
      return null;

    var validDays = ParseDaysOfWeek(DaysOfWeek);

    for(int i = 0; i <= 14; i++) {
      var candidateDate = now.Date.AddDays(i);
      if(candidateDate < StartsAt.Date)
        continue;

      if(!validDays.Contains(candidateDate.DayOfWeek))
        continue;

      var candidate = new DateTimeOffset(
          candidateDate.Year,
          candidateDate.Month,
          candidateDate.Day,
          TimeOfDay.Hour,
          TimeOfDay.Minute,
          TimeOfDay.Second,
          TimeSpan.Zero
      );

      if(candidate <= now)
        continue;

      if(EndsAt.HasValue && candidate > EndsAt.Value)
        continue;

      return candidate;
    }

    return null;
  }

  /// <inheritdoc />
  public override bool IsEligibleToRun() {
    if(!IsValid)
      return false;

    if(MaxExecutions.HasValue && Executions >= MaxExecutions.Value)
      return false;

    if(EndsAt.HasValue && DateTimeOffset.UtcNow > EndsAt.Value)
      return false;

    var next = GetNextExecution();
    return next.HasValue && IsWithinMargin(next.Value);
  }

  /// <inheritdoc />
  public override void UpdateTriggerState() {
    if(EndsAt.HasValue && DateTimeOffset.UtcNow > EndsAt.Value) {
      IsValid = false;
      return;
    }

    Executions++;
    UpdatedAt = DateTimeOffset.UtcNow;
    LastExecution = DateTimeOffset.UtcNow;
    NextExecution = GetNextExecution();
    
    if(MaxExecutions.HasValue && Executions >= MaxExecutions.Value)
      IsValid = false;
  }

  private static HashSet<DayOfWeek> ParseDaysOfWeek(string days) {
    return [.. days
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(d => Enum.Parse<DayOfWeek>(d.Trim(), true))];
  }
}

