namespace GenTaskScheduler.Core.Models.Triggers;

/// <summary>
/// Represents a trigger that runs based on specific month and a specific time of day.
/// </summary>
public class MonthlyTrigger: BaseTrigger {
  /// <summary>
  /// The days of the month on which the task should run. Represented as a comma-separated list (e.g., "1,15,30").
  /// </summary>
  public string DaysOfMonth { get; set; } = string.Empty;

  /// <summary>
  /// The months of the year when the task should run (e.g., "1,7,12" for January, July, and December).
  /// </summary>
  public string MonthsOfYear { get; set; } = string.Empty;

  /// <inheritdoc />
  public override DateTimeOffset? GetNextExecution() {
    var now = DateTimeOffset.UtcNow;
    var baseDate = LastExecution.HasValue && LastExecution.Value > StartsAt ? LastExecution.Value : StartsAt;

    var months = ParseNumbers(MonthsOfYear, 1, 12);
    var days = ParseNumbers(DaysOfMonth, 1, 31);

    for(int i = 0; i < 36; i++) {
      var date = baseDate.AddMonths(i);
      if(!months.Contains(date.Month))
        continue;

      foreach(var day in days.OrderBy(d => d)) {
        if(day > DateTime.DaysInMonth(date.Year, date.Month))
          continue;

        var execution = new DateTimeOffset(date.Year, date.Month, day, TimeOfDay.Hour, TimeOfDay.Minute, TimeOfDay.Second, TimeSpan.Zero);
        if(execution <= now)
          continue;
        if(execution < StartsAt)
          continue;
        if(EndsAt.HasValue && execution > EndsAt.Value)
          continue;

        return execution;
      }
    }

    return null;
  }

  /// <inheritdoc />
  public override bool IsEligibleToRun() {
    var now = DateTimeOffset.UtcNow;
    if(!IsValid || (EndsAt.HasValue && now > EndsAt.Value))
      return false;

    if(MaxExecutions.HasValue && Executions >= MaxExecutions.Value)
      return false;

    var scheduled = GetNextExecution();
    return scheduled.HasValue && now >= scheduled.Value && IsWithinMargin(scheduled.Value);
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

  private static List<int> ParseNumbers(string input, int min, int max) {
    return [.. input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var val) ? val : (int?)null)
                .Where(val => val.HasValue && val.Value >= min && val.Value <= max)
                .Select(val => val!.Value)];
  }
}

