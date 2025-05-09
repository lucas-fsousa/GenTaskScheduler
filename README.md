# GenTaskScheduler.Core

**GenTaskScheduler.Core** is the foundation of the GenTaskScheduler ecosystem. It provides a flexible, extensible, and database-agnostic task scheduling engine with support for multiple trigger types, fluent API construction, and a service-first design to simplify background job execution.

## 📦 Overview

This package contains the core scheduling logic and abstractions for defining tasks, triggers, dependencies, and execution flow. It is intended to be used with provider-specific packages (e.g., `GenTaskScheduler.SqlServer`) for database persistence.

## 🔧 Fluent API Usage

### ✅ Creating a Task

```csharp
var task = GenScheduleTaskBuilder.Create("Daily report generation")
    .WithJob(new MyReportJob { Description = "Generates daily reports" })
    .ConfigureTriggers(triggerBuilder => {
        triggerBuilder.CreateDailyTrigger()
            .SetExecutionTime(new TimeOnly(9, 0))
            .Build();
    })
    .SetIsActive(true)
    .SetAutoDelete(false)
    .SetTimeout(TimeSpan.FromMinutes(10))
    .Build();
```

### 🔁 Creating Triggers with Fluent API

```csharp
var trigger = TriggerBuilder.CreateCronTrigger()
    .SetCronExpression("0 0 * * *")
    .Build();
```

## ⛓ Supported Triggers

### 1. **CronTrigger**

* Uses the [Cronos](https://github.com/HangfireIO/Cronos) library for maximum compatibility.
* Ideal for flexible and complex schedules.
* Example:

  ```csharp
  triggerBuilder.CreateCronTrigger()
      .SetCronExpression("*/5 * * * *")
      .Build();
  ```

### 2. **OnceTrigger**

* Executes a task only once at a specific moment in time.
* Example:

  ```csharp
  triggerBuilder.CreateOnceTrigger()
      .SetExecutionTime(DateTimeOffset.UtcNow.AddMinutes(5))
      .Build();
  ```

### 3. **DailyTrigger**

* Triggers daily at a specified time.
* Example:

  ```csharp
  triggerBuilder.CreateDailyTrigger()
      .SetExecutionTime(new TimeOnly(7, 30))
      .Build();
  ```

### 4. **IntervalTrigger**

* Runs repeatedly within a time window with a fixed interval.
* Example:

  ```csharp
  triggerBuilder.CreateIntervalTrigger()
      .SetStartTime(DateTimeOffset.UtcNow)
      .SetEndTime(DateTimeOffset.UtcNow.AddHours(1))
      .SetExecutionInterval(TimeSpan.FromMinutes(10))
      .Build();
  ```

### 5. **WeeklyTrigger**

* Triggers on specific days of the week at defined times.
* Example:

  ```csharp
  triggerBuilder.CreateWeeklyTrigger()
      .SetExecutionDays([DayOfWeek.Monday, DayOfWeek.Friday])
      .SetExecutionTime(new TimeOnly(8, 0))
      .Build();
  ```

### 6. **MonthlyTrigger**

* Executes on specific days and months.
* Example:

  ```csharp
  triggerBuilder.CreateMonthlyTrigger()
      .SetMonths([1, 6, 12])
      .SetDays([1, 15])
      .SetExecutionTime(new TimeOnly(10, 0))
      .Build();
  ```

### 7. **CalendarTrigger**

* Fully manual configuration with specific date-time entries.
* Example:

  ```csharp
  triggerBuilder.CreateCalendarTrigger(DateTimeOffset.UtcNow.AddMinutes(1))
      .AddCalendarEntries([
          new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddMinutes(2) },
          new CalendarEntry { ScheduledDateTime = DateTimeOffset.UtcNow.AddHours(1) }
      ])
      .Build();
  ```

## 📚 Key Interfaces

* `ITaskRepository`: Create, update, delete, and query tasks.
* `ITriggerRepository`: Manage triggers linked to tasks.
* `ITaskHistoryRepository`: Log and retrieve execution history.
* `ISchedulerLauncher`: Controls execution of pending tasks.
* `ISchemeProvider`: Extracts schema details for manual migrations.

## 🧠 Best Practices

* Enable `AutoDeleteInactiveTasks` to keep the database clean from unused entries.
* Use `CalendarTrigger` for custom schedules instead of forcing logic into other trigger types.
* Avoid overlapping triggers unless intended.
* Prefer smaller intervals with fail-safe retry logic for critical tasks.

## 🔗 Related Packages

* [GenTaskScheduler (Core Repository)](https://github.com/lucas-fsousa/GenTaskScheduler)
* [GenTaskScheduler.SqlServer](https://github.com/lucas-fsousa/GenTaskScheduler.SqlServer)

## 📖 Learn More

* [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
* [Cron Expression Cheatsheet](https://crontab.guru/)

---

With **GenTaskScheduler.Core**, you’re free to design your task execution workflows with precision, clarity, and complete extensibility.

---

Need help? Submit an issue or open a discussion on [GitHub](https://github.com/your-org/GenTaskScheduler).
