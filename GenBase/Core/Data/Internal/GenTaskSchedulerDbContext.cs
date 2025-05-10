using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.EntityFrameworkCore;

namespace GenTaskScheduler.Core.Data.Internal;

/// <inheritdoc />
public abstract class GenTaskSchedulerDbContext: DbContext {
  protected GenTaskSchedulerDbContext(DbContextOptions options) : base(options) { }
  protected GenTaskSchedulerDbContext() { }
  public DbSet<ScheduledTask> ScheduledTasks { get; set; }

  // TPH hierarchy for triggers
  public DbSet<BaseTrigger> BaseTriggers { get; set; }
  public DbSet<CronTrigger> CronTriggers { get; set; }
  public DbSet<OnceTrigger> OnceTriggers { get; set; }
  public DbSet<IntervalTrigger> IntervalTriggers { get; set; }
  public DbSet<MonthlyTrigger> DailyWeeklyMonthlyTriggers { get; set; }
  public DbSet<CalendarTrigger> CalendarTriggers { get; set; }

  public DbSet<TaskExecutionHistory> TaskExecutionsHistory { get; set; }
  public DbSet<CalendarEntry> CalendarEntries { get; set; }

  /// <inheritdoc/>
  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.SetupScheduledTask();
    modelBuilder.SetupTriggerMappings();
    base.OnModelCreating(modelBuilder);
  }
}
