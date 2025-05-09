using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Builder.TaskBuilder;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Scheduler;
using GenTaskScheduler.Test.Helpers;
using GenTaskScheduler.Test.Models.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GenTaskScheduler.Test.Scheduler.Tests;
public class SchedulerTests {
  private readonly Mock<ITaskRepository> _taskRepoMock = new();
  private readonly Mock<ITriggerRepository> _triggerRepoMock = new();
  private readonly Mock<ITaskHistoryRepository> _historyRepoMock = new();
  private readonly ILogger<ApplicationLogger> _logger = new NullLogger<ApplicationLogger>();
  private readonly ServiceProvider _serviceProvider;

  public SchedulerTests() {
    var services = new ServiceCollection();

    // Simule serviços do escopo
    services.AddTransient(_ => _taskRepoMock.Object);
    services.AddTransient(_ => _triggerRepoMock.Object);
    services.AddTransient(_ => _historyRepoMock.Object);

    _serviceProvider = services.BuildServiceProvider();
  }

  [Fact]
  public void Should_Create_Cron_Task_With_Valid_Trigger() {
    var task = GenTaskTestFactory.CreateCronTask("0/5 * * * *");
    Assert.Single(task.Triggers);
    Assert.Contains("cron", task.Triggers.First().TriggerDescription, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Should_Create_Daily_Task_With_Valid_Trigger() {
    var task = GenTaskTestFactory.CreateDailyTask(TimeOnly.FromDateTime(DateTime.Now.AddMinutes(1)));
    Assert.Single(task.Triggers);
    Assert.Contains("daily", task.Triggers.First().TriggerDescription, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Should_Create_Weekly_Task_With_Valid_Trigger() {
    var task = GenTaskTestFactory.CreateWeeklyTask([DayOfWeek.Monday, DayOfWeek.Friday], new TimeOnly(9, 0));
    Assert.Single(task.Triggers);
    Assert.Contains("weekly", task.Triggers.First().TriggerDescription, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Should_Create_Monthly_Task_With_Valid_Trigger() {
    var task = GenTaskTestFactory.CreateMonthlyTask(
      [new IntRange(1, 5)],
      [MonthOfYear.January, MonthOfYear.March],
      new TimeOnly(10, 0)
    );
    Assert.Single(task.Triggers);
    Assert.Contains("monthly", task.Triggers.First().TriggerDescription, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Should_Create_Task_With_Multiple_Triggers() {
    var task = GenTaskTestFactory.CreateTaskWithMultipleTriggers([
      GenTaskTestFactory.CreateDailyTrigger(TimeOnly.FromDateTime(DateTime.Now.AddMinutes(2))),
    GenTaskTestFactory.CreateCronTrigger("*/10 * * * *"),
    GenTaskTestFactory.CreateWeeklyTrigger([DayOfWeek.Monday], new TimeOnly(9, 0))
    ]);

    Assert.Equal(3, task.Triggers.Count);
    Assert.Contains(task.Triggers, t => t.Description.Contains("daily", StringComparison.OrdinalIgnoreCase));
    Assert.Contains(task.Triggers, t => t.Description.Contains("cron", StringComparison.OrdinalIgnoreCase));
    Assert.Contains(task.Triggers, t => t.Description.Contains("weekly", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public void Should_Create_Task_With_Multiple_Valid_Triggers_And_AutoDelete_False() {
    var task = GenTaskTestFactory.CreateTaskWithMultipleTriggers([
      GenTaskTestFactory.CreateCronTrigger("0 12 * * 1"),
    GenTaskTestFactory.CreateMonthlyTrigger(
      [new IntRange(1, 10)],
      [MonthOfYear.January, MonthOfYear.December],
      new TimeOnly(8, 30))
    ], autoDelete: false);

    Assert.Equal(2, task.Triggers.Count);
    Assert.False(task.AutoDelete);
    Assert.All(task.Triggers, t => Assert.False(t.AutoDelete));
  }

  [Fact]
  public void Should_Create_Task_With_No_Triggers() {
    var task = GenTaskTestFactory.CreateEmptyTask("Task sem triggers");
    Assert.Empty(task.Triggers);
  }

  [Fact]
  public void Should_Create_Task_With_Dependency_On_Another_Task() {
    var masterTask = GenTaskTestFactory.CreateOnceTask("Tarefa Principal", DateTimeOffset.UtcNow.AddMinutes(1));

    var dependentTask = GenScheduleTaskBuilder.Create("Tarefa Dependente")
      .WithJob(new JobExec { Descricao = "Depende da principal" })
      .ConfigureTriggers(builder =>
        builder.CreateOnceTrigger()
               .SetExecutionDateTime(DateTimeOffset.UtcNow.AddMinutes(5))
               .SetDescription("Trigger dependente")
               .Build()
      )
      .DependsOn(masterTask)
      .WithStatus(GenTaskHistoryStatus.Success)
      .SetAutoDelete(true)
      .SetIsActive(true)
      .Build();

    Assert.Single(dependentTask.DependsOn);
    Assert.Contains(GenTaskHistoryStatus.Success.ToString(), dependentTask.DependsOn.First().RequiredStatuses);
  }

  [Fact]
  public void Should_Create_Trigger_With_Max_Executions_Limit() {
    var trigger = GenTaskTestFactory.CreateIntervalTrigger(
      start: DateTimeOffset.UtcNow.AddSeconds(30),
      interval: TimeSpan.FromSeconds(10),
      maxExecutions: 3
    );

    Assert.Equal(3, trigger.MaxExecutions);
    Assert.Equal(0, trigger.Executions);
  }

  [Fact]
  public void Should_Create_Task_With_SelfDeleting_Trigger_After_Max_Executions() {
    var task = GenScheduleTaskBuilder.Create("Recorrente com Limite")
      .WithJob(new JobExec { JobName = "Job com limite" })
      .ConfigureTriggers(builder =>
        builder.CreateIntervalTrigger(DateTimeOffset.UtcNow.AddSeconds(10))
               .SetRepeatInterval(TimeSpan.FromSeconds(5))
               .SetExecutionLimit(5)
               .SetAutoDelete(true)
               .Build()
      )
      .SetIsActive(true)
      .SetAutoDelete(false) // Task permanece, trigger se apaga
      .Build();

    var trigger = task.Triggers.First();
    Assert.Equal(5, trigger.MaxExecutions);
    Assert.True(trigger.AutoDelete);
    Assert.False(task.AutoDelete);
  }


}
