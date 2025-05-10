# GenTaskScheduler

**GenTaskScheduler** is the foundational library of the GenTaskScheduler ecosystem. It delivers a flexible, extensible, and database-agnostic task scheduling engine designed for modern .NET applications. With support for multiple trigger types, fluent API construction, and a service-oriented architecture, it simplifies the creation and management of background jobs across diverse environments.

---

## ✨ Overview

This package provides the core scheduling logic and abstractions needed to define tasks, configure triggers, establish dependencies, and control execution flow. It is designed to work seamlessly with provider-specific packages (e.g., `GenTaskScheduler.SqlServer`) for data persistence and database operations.

---

## 🔧 Fluent API: Defining and Scheduling Tasks

### ✅ Creating a Basic Task

```csharp
var task = GenScheduleTaskBuilder.Create("A test calendar task")
  .WithJob(new Job())
  .ConfigureTriggers(triggerBuilder => {
    triggerBuilder.CreateCalendarTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
      .AddCalendarEntries([
        new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddMinutes(2) }
      ])
      .Build();
  })
  .NotDepends()
  .SetAutoDelete(false)
  .SetIsActive(true)
  .SetTimeout(TimeSpan.FromSeconds(20))
  .Build();
```

### ⚖️ Creating a Dependent Task

```csharp
// This task will only be eligible to execute if the parent task's last execution failed.
var taskDependant = GenScheduleTaskBuilder.Create("Weekly Trigger Task")
  .WithJob(new Job())
  .ConfigureTriggers(triggerBuilder => {
    triggerBuilder.CreateWeeklyTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
      .SetDaysOfWeek(DayOfWeek.Sunday, DayOfWeek.Saturday)
      .SetTimeOfDay(new TimeOnly(22, 00))
      .Build();
  })
  .DependsOn(myOtherTask)
  .WithStatus(GenTaskHistoryStatus.Failed)
  .Build();
```

---

## 🔁 Creating Triggers with the Fluent API

### Example:

```csharp
var trigger = TriggerBuilder.CreateCronTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetCronExpression("0 0 * * *")
  .Build();
```

---

## ⚓ Supported Trigger Types

### 1. **CronTrigger**

* Uses the [Cronos](https://github.com/HangfireIO/Cronos) library.
* Ideal for flexible and complex recurring schedules.

```csharp
triggerBuilder.CreateCronTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetCronExpression("*/5 * * * *") // Every 5 minutes
  .Build();
```

### 2. **OnceTrigger**

* Executes the task once at a specific datetime.

```csharp
triggerBuilder.CreateOnceTrigger()
  .SetExecutionDateTime(DateTimeOffset.UtcNow.AddMinutes(5))
  .Build();
```

### 3. **DailyTrigger**

* Executes daily at a defined time of day.

```csharp
triggerBuilder.CreateDailyTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetTimeOfDay(new TimeOnly(7, 30)) // Executes at 7:30 AM daily
  .Build();
```

### 4. **IntervalTrigger**

* Executes repeatedly over a specified time window with a fixed interval.

```csharp
triggerBuilder.CreateIntervalTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetRepeatIntervalMinutes(5)
  .SetExecutionLimit(5) // Optional: max number of executions
  .SetValidity(DateTimeOffset.UtcNow.AddHours(1)) // Disables after 1 hour
  .Build();
```

### 5. **WeeklyTrigger**

* Executes on selected days of the week at a specific time.

```csharp
triggerBuilder.CreateWeeklyTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetDaysOfWeek(DayOfWeek.Sunday, DayOfWeek.Saturday)
  .SetTimeOfDay(new TimeOnly(22, 00))
  .Build();
```

### 6. **MonthlyTrigger**

* Executes on specific days and months.

```csharp
var triggerBuilder = GenSchedulerTriggerBuilder.Start(myTask);

triggerBuilder.CreateMonthlyTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetDaysOfMonth(1, 2, 7, 9, 15, 0) // 0 = last day of the month
  .SetMonthsOfYear(MonthOfYear.January, MonthOfYear.March, MonthOfYear.April)
  .SetTimeOfDay(new(22, 45))
  .Build();

// Using IntRange for more flexibility
triggerBuilder.CreateMonthlyTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .SetDaysOfMonth(new IntRange(1, 9), IntRange.Zero) // IntRange.Zero = last day of the month. IntRange(1, 9) = [1,2,3,4,5,6,7,8,9]
  .SetMonthsOfYear(MonthOfYear.August)
  .SetTimeOfDay(new(0, 0))
  .Build();
```

### 7. **CalendarTrigger**

* Manually configured with specific datetime entries.
* Useful for sporadic or highly specific execution needs.

```csharp
triggerBuilder.CreateCalendarTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
  .AddCalendarEntries([
    new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddMinutes(2) },
    new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddHours(1) },
    new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddDays(15) },
    new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddMonths(2) }
  ])
  .Build();
```

---

## 📚 Defining Custom Jobs

```csharp
// A custom job implementing IJob
public class MultiplyAnyNumber : IJob {
  public int N1 { get; set; }
  public int N2 { get; set; }

  public Task ExecuteJobAsync(CancellationToken cancellationToken = default) {
    Console.WriteLine("JOB starting");
    Console.WriteLine($"{N1} x {N2} = {N1 * N2}");
    return Task.CompletedTask;
  }
}
```

---

## 🔎 Core Interfaces

* `ITaskRepository`: CRUD operations for tasks.
* `ITriggerRepository`: Manage triggers assigned to tasks.
* `ITaskHistoryRepository`: Track and retrieve task execution history.
* `ISchedulerLauncher`: Launches and controls pending task executions.
* `ISchemeProvider`: Schema metadata for manual database migrations.
* `IGenTaskSchedulerDatabaseProvider`: Provider abstraction for database configuration and migrations.
* `IJob`: Interface for job logic executed during task runs.

---

## 🧠 Best Practices

* Enable `AutoDeleteInactiveTasks` to keep your database clean.
* Prefer `CalendarTrigger` for arbitrary or non-repeating schedules.
* Avoid overlapping triggers unless explicitly required.
* Use small intervals combined with retry logic for high-availability scenarios.

---

## 🔗 Related Packages

* [GenTaskScheduler (Core)](https://github.com/lucas-fsousa/GenTaskScheduler)
* [GenTaskScheduler.SqlServer](https://github.com/lucas-fsousa/GenTaskScheduler.SqlServer)

---

## 📖 Learn More

* [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
* [Cron Expression Cheatsheet](https://crontab.guru/)

---

With **GenTaskScheduler**, you gain complete control and flexibility to orchestrate complex background jobs with elegance, extensibility, and precision.

---

Need help or want to contribute? [Open an issue or discussion on GitHub](https://github.com/lucas-fsousa/GenTaskScheduler).
