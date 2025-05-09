using System;

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
    var searchStart = now > StartsAt ? now : StartsAt;

    var months = ParseNumbers(MonthsOfYear, 1, 12);
    var rawDays = ParseNumbers(DaysOfMonth, 0, 31);

    // Começa no primeiro dia do mês da data base
    var initialMonth = new DateTimeOffset(searchStart.Year, searchStart.Month, 1, 0, 0, 0, TimeSpan.Zero);

    for(int i = 0; i < 36; i++) {
      var date = initialMonth.AddMonths(i);
      if(!months.Contains(date.Month))
        continue;

      var validDays = ExpandDaysForMonth(date.Year, date.Month, rawDays);

      foreach(var day in validDays) {
        if(day > DateTime.DaysInMonth(date.Year, date.Month))
          continue;

        var execution = new DateTimeOffset(date.Year, date.Month, day, TimeOfDay.Hour, TimeOfDay.Minute, TimeOfDay.Second, TimeSpan.Zero);

        if(execution < now && !IsWithinMargin(execution))
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
    var result = new List<int>();

    foreach(var token in input.Split(',', StringSplitOptions.RemoveEmptyEntries)) {
      var trimmed = token.Trim();

      if(trimmed.Contains('-')) {
        var parts = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length == 2 &&
            int.TryParse(parts[0].Trim(), out int start) &&
            int.TryParse(parts[1].Trim(), out int end)) {
          if(start > end)
            (start, end) = (end, start);

          for(int i = start; i <= end; i++) {
            if(i >= min && i <= max)
              result.Add(i);
          }
        }
      } else if(int.TryParse(trimmed, out int value)) {
        if(value >= min && value <= max)
          result.Add(value);
      }
    }

    return result;
  }

  private static IEnumerable<int> ExpandDaysForMonth(int year, int month, List<int> days) {
    var lastDay = DateTime.DaysInMonth(year, month);
    return days.Order().Select(d => d == 0 ? lastDay : d).Where(d => d <= lastDay);
  }
}

