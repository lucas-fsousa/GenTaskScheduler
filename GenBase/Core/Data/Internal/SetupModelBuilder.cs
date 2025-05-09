using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.EntityFrameworkCore;

namespace GenTaskScheduler.Core.Data.Internal;
internal static class SetupModelBuilder {

  internal static void SetupTriggerMappings(this ModelBuilder modelBuilder) {
    modelBuilder.Entity<BaseTrigger>()
      .ToTable("Triggers")
      .HasDiscriminator<string>("TriggerType")
      .HasValue<CronTrigger>("Cron")
      .HasValue<OnceTrigger>("Once")
      .HasValue<DailyTrigger>("Daily")
      .HasValue<WeeklyTrigger>("Weekly")
      .HasValue<MonthlyTrigger>("Monthly")
      .HasValue<IntervalTrigger>("Interval")
      .HasValue<CalendarTrigger>("Calendar");

    modelBuilder.Entity<BaseTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.TriggerDescription).HasMaxLength(100);
      entity.Property(e => e.LastTriggeredStatus).HasMaxLength(20);
    });

    modelBuilder.SetupCronTrigger();
    modelBuilder.SetupOnceTrigger();
    modelBuilder.SetupDailyTrigger();
    modelBuilder.SetupWeeklyTrigger();
    modelBuilder.SetupMonthlyTrigger();
    modelBuilder.SetupIntervalTrigger();
    modelBuilder.SetupCalendarTrigger();
    modelBuilder.SetupExecutionHistory();
  }

  internal static ModelBuilder SetupExecutionHistory(this ModelBuilder builder) {
    builder.Entity<TaskExecutionHistory>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.StartedAt).IsRequired();
      entity.Property(e => e.EndedAt).IsRequired();
      entity.Property(e => e.Status).IsRequired();
      entity.Property(e => e.ErrorMessage);
      entity.Property(e => e.Status).HasMaxLength(100);

      entity.Ignore(e => e.ResultObject);
    });

    return builder;
  }

  internal static ModelBuilder SetupScheduledTask(this ModelBuilder builder) {
    builder.Entity<ScheduledTask>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
      entity.Property(e => e.CreatedAt).IsRequired();
      entity.Property(e => e.AutoDelete).IsRequired();
      entity.Property(e => e.IsActive).IsRequired();
      entity.Property(e => e.BlobArgs).IsRequired();
      entity.Property(e => e.ExecutionStatus).HasMaxLength(15).IsRequired();

      entity.HasOne(e => e.DependsOnTask)
        .WithMany()
        .HasForeignKey(e => e.DependsOnTaskId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.LastExecutionHistory)
        .WithMany()
        .HasForeignKey(e => e.LastExecutionHistoryId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.Navigation(e => e.Triggers).AutoInclude();
      entity.Navigation(e => e.DependsOnTask).AutoInclude(false);
    });

    return builder;
  }

  internal static ModelBuilder SetupCalendarTrigger(this ModelBuilder builder) {
    builder.Entity<CalendarTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Navigation(e => e.CalendarEntries).AutoInclude(true);
    });

    builder.Entity<CalendarEntry>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.ScheduledDateTime).IsRequired();
    });


    return builder;
  }

  internal static ModelBuilder SetupCronTrigger(this ModelBuilder builder) {
    builder.Entity<CronTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.CronExpression).HasMaxLength(100).IsRequired();
    });
    return builder;
  }

  internal static ModelBuilder SetupOnceTrigger(this ModelBuilder builder) {
    builder.Entity<OnceTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.StartsAt).IsRequired();
    });

    return builder;
  }

  internal static ModelBuilder SetupIntervalTrigger(this ModelBuilder builder) {
    builder.Entity<IntervalTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.StartsAt).IsRequired();
    });

    return builder;
  }

  internal static ModelBuilder SetupMonthlyTrigger(this ModelBuilder builder) {
    builder.Entity<MonthlyTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.TimeOfDay).IsRequired();
      entity.Property(e => e.DaysOfMonth).HasMaxLength(100);
    });

    return builder;
  }

  internal static ModelBuilder SetupDailyTrigger(this ModelBuilder builder) {
    builder.Entity<DailyTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.TimeOfDay).IsRequired();
    });

    return builder;
  }

  internal static ModelBuilder SetupWeeklyTrigger(this ModelBuilder builder) {
    builder.Entity<WeeklyTrigger>(entity => {
      entity.Property(e => e.Id).ValueGeneratedNever();
      entity.Property(e => e.TimeOfDay).IsRequired();
      entity.Property(e => e.DaysOfWeek).HasMaxLength(100);
    });

    return builder;
  }
}

