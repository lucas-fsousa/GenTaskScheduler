using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Helper;
internal static class InternalHelperExtensions {
  /// <summary>
  /// Internal method to define the days of the week that a trigger should be fired. Only for valid triggers.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="daysOfWeek">Days of the week for inclusion</param>
  /// <returns>Current instance of Trigger</returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
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

  /// <summary>
  /// Internal method to set the trigger description.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="description">Trigger description, optional.</param>
  /// <returns>Current instance of Trigger</returns>
  internal static T InternalSetDescription<T>(this T current, string description) where T : BaseTrigger {
    current!.TriggerDescription = description;
    return current;
  }

  /// <summary>
  /// Internal method to set the trigger validity.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="endsAt">Optional value for expiration date. If null, the trigger has no expiration date.</param>
  /// <returns>Current instance of Trigger</returns>
  /// <exception cref="ArgumentNullException"></exception>
  internal static T InternalSetValidity<T>(this T current, DateTimeOffset? endsAt = null) where T : BaseTrigger {
    if(endsAt is not null && current.StartsAt > endsAt)
      throw new ArgumentException("Invalid trigger time range. EndsAt must be after StartsAt or EndsAt must be null.");

    current!.EndsAt = endsAt;
    return current;
  }

  /// <summary>
  ///  Internal method to set the start date of a trigger.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="startsAt">Mandatory value for the start of validity</param>
  /// <returns>Current instance of Trigger</returns>
  /// <exception cref="ArgumentNullException"></exception>
  internal static T InternalSetStartDate<T>(this T current, DateTimeOffset startsAt) where T: BaseTrigger {
    if(startsAt < DateTimeOffset.UtcNow)
      throw new ArgumentException("Start date cannot be in the past.", nameof(startsAt));

    current!.StartsAt = startsAt;
    return current;
  }

  /// <summary>
  /// Internal method to define whether the trigger should be deleted after being fired..
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="autoDelete">Flag value to set</param>
  /// <returns>Current instance of Trigger</returns>
  internal static T InternalSetAutoDelete<T>(this T current, bool autoDelete) where T : BaseTrigger {
    current!.ShouldAutoDelete = autoDelete;
    return current;
  }

  /// <summary>
  /// Internal method to sets the max number of executions for a valid trigger.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="maxExecutions">Number of executions to set</param>
  /// <returns>Current instance of Trigger</returns>
  internal static T InternalSetMaxExecutionLimit<T>(this T current, int? maxExecutions) where T : BaseTrigger {
    current!.MaxExecutions = maxExecutions;
    return current;
  }

  /// <summary>
  /// Internal method to sets the days of months for a valid trigger.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="daysOfMonth">Days of month for execution</param>
  /// <returns>Current instance of Trigger</returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  internal static T InternalSetDaysOfMonth<T>(this T current, params int[] daysOfMonth) where T : BaseTrigger {
    if(daysOfMonth is null || daysOfMonth.Length == 0)
      throw new ArgumentNullException(nameof(daysOfMonth), "Days of month cannot be null or empty.");

    if(current is MonthlyTrigger mt)
      mt.DaysOfMonth = string.Join(',', daysOfMonth);
    else
      throw new InvalidOperationException($"Invalid trigger type '{current.GetType().Name}'. Expected MonthlyTrigger.");

    return current;
  }

  /// <summary>
  /// Internal method to sets the monthly of year for a valid trigger.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="monthsOfYear">Months of year for execution</param>
  /// <returns>Current instance of Trigger </returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  internal static T InternalSetMonthsOfYear<T>(this T current, params MonthOfYear[] monthsOfYear) where T : BaseTrigger {
    if(monthsOfYear is null || monthsOfYear.Length == 0)
      throw new ArgumentNullException(nameof(monthsOfYear), "Months of year cannot be null or empty.");

    if(current is MonthlyTrigger mt)
      mt.MonthsOfYear = string.Join(',', monthsOfYear.Select(m => (int)m));
    else
      throw new InvalidOperationException($"Invalid trigger type '{current.GetType().Name}'. Expected MonthlyTrigger.");

    return current;
  }

  /// <summary>
  /// Internal method to sets the time of day for a valid trigger.
  /// </summary>
  /// <typeparam name="T">Type of trigger</typeparam>
  /// <param name="current">Instance of the current trigger</param>
  /// <param name="time">Time of day for execution</param>
  /// <returns>Current instance of Trigger </returns>
  internal static T InternalSetTimeOfDay<T>(this T current, TimeOnly time) where T : BaseTrigger {
    if(current is MonthlyTrigger mt)
      mt.TimeOfDay = time;

    return current;
  }

  /// <summary>
  /// Converts a <see cref="TimeOnly"/> object to a <see cref="DateTimeOffset"/> object with the specified date offset.
  /// </summary>
  /// <param name="time">TimeOnly value to parse</param>
  /// <param name="date">Date offset reference</param>
  /// <returns>A new <see cref="DateTimeOffset"/></returns>
  public static DateTimeOffset TimeOnlyToDateTimeOffset(this TimeOnly time, DateTimeOffset date) {
    return new DateTimeOffset(
        date.Year,
        date.Month,
        date.Day,
        time.Hour,
        time.Minute,
        time.Second,
        TimeSpan.Zero
    );
  }

  /// <summary>
  /// Converts a TimeOnly object to a  <see cref="DateTime"/> object with the specified date.
  /// </summary>
  /// <param name="time">TimeOnly value to parse</param>
  /// <param name="date">Date reference</param>
  /// <returns>A new <see cref="DateTime"/></returns>
  public static DateTime TimeOnlyToDateTime(this TimeOnly time, DateTime date) {
    return new DateTime(
        date.Year,
        date.Month,
        date.Day,
        time.Hour,
        time.Minute,
        time.Second
    );
  }
}

