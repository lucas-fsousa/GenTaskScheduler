using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
internal static class BaseTriggerBuilderValidator {
  internal static T InternalSetDaysOfWeek<T>(this T current, params DayOfWeek[] daysOfWeek) where T : BaseTrigger {
    if(daysOfWeek is null || daysOfWeek.Length == 0)
      throw new ArgumentNullException(nameof(daysOfWeek), "Days of week cannot be null or empty.");

    var tmp = string.Join(',', daysOfWeek.Select(m => m.ToString()));
    if(current is WeeklyTrigger wt)
      wt.DaysOfWeek = tmp;
    else
      throw new InvalidOperationException($"Invalid trigger type '{current.GetType().Name}'. Expected WeeklyTrigger.");

    return current;
  }

  internal static T InternalSetDescription<T>(this T current, string description) where T : BaseTrigger {
    current!.TriggerDescription = description;
    return current;
  }

  internal static T InternalSetValidity<T>(this T current, DateTimeOffset startsAt, DateTimeOffset? endsAt = null) where T : BaseTrigger {
    if(startsAt < DateTimeOffset.UtcNow || endsAt is not null && startsAt > endsAt)
      throw new ArgumentException("Invalid trigger time range. StartsAt must be in the future and EndsAt must be after StartsAt or EndsAt must be null.");

    current!.StartsAt = startsAt;
    current!.EndsAt = endsAt;
    return current;
  }

  internal static T InternalSetAutoDelete<T>(this T current, bool autoDelete) where T : BaseTrigger {
    current!.ShouldAutoDelete = autoDelete;
    return current;
  }

  internal static T InternalSetMaxExecutionLimit<T>(this T current, int? maxExecutions) where T : BaseTrigger {
    current!.MaxExecutions = maxExecutions;
    return current;
  }

  internal static T InternalSetDaysOfMonth<T>(this T current, params int[] daysOfMonth) where T : BaseTrigger {
    if(daysOfMonth is null || daysOfMonth.Length == 0)
      throw new ArgumentNullException(nameof(daysOfMonth), "Days of month cannot be null or empty.");

    if(current is MonthlyTrigger mt)
      mt.DaysOfMonth = string.Join(',', daysOfMonth);
    else
      throw new InvalidOperationException($"Invalid trigger type '{current.GetType().Name}'. Expected MonthlyTrigger.");

    return current;
  }

  public static T InternalSetMonthsOfYear<T>(this T current, params MonthOfYear[] monthOfYears) where T : BaseTrigger {
    if(monthOfYears is null || monthOfYears.Length == 0)
      throw new ArgumentNullException(nameof(monthOfYears), "Months of year cannot be null or empty.");

    if(current is MonthlyTrigger mt)
      mt.MonthsOfYear = string.Join(',', monthOfYears.Select(m => m.ToString()));
    else
      throw new InvalidOperationException($"Invalid trigger type '{current.GetType().Name}'. Expected MonthlyTrigger.");

    return current;
  }

  public static T InternalSetTimeOfDay<T>(this T current, TimeSpan time) where T : BaseTrigger {
    if(current is MonthlyTrigger mt)
      mt.TimeOfDay = time;

    return current;
  }
}

