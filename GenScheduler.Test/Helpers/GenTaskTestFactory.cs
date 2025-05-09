using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Builder.TaskBuilder;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Test.Models.Jobs;

namespace GenTaskScheduler.Test.Helpers;

internal static class GenTaskTestFactory {
  internal static ScheduledTask CreateOnceTask(DateTimeOffset? execTime = null) {
    execTime ??= DateTimeOffset.UtcNow.AddMinutes(1);

    return GenScheduleTaskBuilder.Create($"Once Task idx{execTime.Value.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Fake Job Execution" })
        .ConfigureTriggers(tb =>
            tb.CreateOnceTrigger()
              .SetExecutionDateTime(execTime.Value)
              .SetDescription("Single execution trigger")
              .SetAutoDelete(true)
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(15))
        .SetIsActive(true)
        .Build();
  }

  internal static ScheduledTask CreateIntervalTask(int repeatCount = 3, DateTimeOffset? execTime = null) {
    execTime ??= DateTimeOffset.UtcNow.AddMinutes(1);

    return GenScheduleTaskBuilder.Create($"Interval Task idx{execTime.Value.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Repeat job" })
        .ConfigureTriggers(tb =>
            tb.CreateIntervalTrigger(execTime.Value)
              .SetRepeatIntervalMinutes(1)
              .SetExecutionLimit(repeatCount)
              .SetDescription("Interval Trigger")
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(10))
        .SetIsActive(true)
        .Build();
  }

  internal static ScheduledTask CreateCalendarTaskWithMultipleEntries(params DateTimeOffset[] entriesTimers) {
    var execTime = DateTimeOffset.UtcNow.AddMinutes(1);
    return GenScheduleTaskBuilder.Create($"Calendar Task idx{execTime.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Calendar job" })
        .ConfigureTriggers(tb =>
            tb.CreateCalendarTrigger(DateTimeOffset.UtcNow)
              .AddCalendarEntries([.. entriesTimers.Select(e => new CalendarEntry { ScheduledDateTime = e })])
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(30))
        .SetIsActive(true)
        .Build();
  }

  internal static ScheduledTask CreateCronTask(string cron = "*/1 * * * *", DateTimeOffset? start = null) {
    start ??= DateTimeOffset.UtcNow;

    return GenScheduleTaskBuilder.Create($"Cron Task idx{start.Value.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Cron job" })
        .ConfigureTriggers(tb =>
            tb.CreateCronTrigger(start.Value)
              .SetCronExpression(cron)
              .SetDescription("Cron trigger")
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(20))
        .SetIsActive(true)
        .Build();
  }

  internal static ScheduledTask CreateDailyTask(TimeOnly? time = null, DateTimeOffset? start = null) {
    start ??= DateTimeOffset.UtcNow;
    time ??= TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(1));

    return GenScheduleTaskBuilder.Create($"Daily Task idx{start.Value.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Daily job" })
        .ConfigureTriggers(tb =>
            tb.CreateDailyTrigger(start.Value)
              .SetTimeOfDay(time.Value)
              .SetDescription("daily trigger")
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(25))
        .SetIsActive(true)
        .Build();
  }

  internal static ScheduledTask CreateWeeklyTask(DayOfWeek[] days, TimeOnly? time = null, DateTimeOffset? start = null) {
    start ??= DateTimeOffset.UtcNow;
    time ??= TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(1));

    return GenScheduleTaskBuilder.Create($"Weekly Task idx{start.Value.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Weekly job" })
        .ConfigureTriggers(tb =>
            tb.CreateWeeklyTrigger(start.Value)
              .SetDaysOfWeek(days)
              .SetTimeOfDay(time.Value)
              .SetDescription("weekly trigger")
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(25))
        .SetIsActive(true)
        .Build();
  }

  internal static ScheduledTask CreateMonthlyTask(IntRange[] days, MonthOfYear[] months, TimeOnly? time = null, DateTimeOffset? start = null) {
    start ??= DateTimeOffset.UtcNow;
    time ??= TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(1));

    return GenScheduleTaskBuilder.Create($"Monthly Task idx{start.Value.Ticks:x2}")
        .WithJob(new JobExec { JobName = "Monthly job" })
        .ConfigureTriggers(tb =>
            tb.CreateMonthlyTrigger(start.Value)
              .SetDaysOfMonth(days)
              .SetMonthsOfYear(months)
              .SetTimeOfDay(time.Value)
              .SetDescription("monthly trigger")
              .Build()
        )
        .NotDepends()
        .SetTimeout(TimeSpan.FromSeconds(30))
        .SetIsActive(true)
        .Build();
  }
}
