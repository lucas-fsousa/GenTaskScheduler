﻿using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Cron;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

/// <summary>
/// Builder class for creating triggers in the GenScheduler.
/// </summary>
public partial class GenSchedulerTriggerBuilder:
  ICommonTriggerStep,
  ITriggerBuilderStart {
  internal readonly ScheduledTask _task;
  internal BaseTrigger? _current;
  private GenSchedulerTriggerBuilder(ScheduledTask task) => _task = task;

  /// <summary>
  /// Creates a new trigger builder instance for the specified task.
  /// </summary>
  /// <param name="task">Scheduled Task that will receive the created triggers</param>
  /// <returns><see cref="ITriggerBuilderStart"/></returns>
  public static ITriggerBuilderStart Start(ScheduledTask task) => new GenSchedulerTriggerBuilder(task);

  /// <inheritdoc />
  public ICommonTriggerStep SetValidity(DateTimeOffset? endsAt = null) {
    _current!.InternalSetValidity(endsAt);
    return this;
  }

  /// <inheritdoc />
  public ICommonTriggerStep SetAutoDelete(bool autoDelete) {
    _current!.InternalSetAutoDelete(autoDelete);
    return this;
  }

  /// <inheritdoc />
  public ICommonTriggerStep SetDescription(string description) {
    _current!.InternalSetDescription(description);
    return this;
  }

  /// <inheritdoc />
  public ICommonTriggerStep SetExecutionLimit(int? maxExecutions) {
    _current!.InternalSetMaxExecutionLimit(maxExecutions);
    return this;
  }

  /// <inheritdoc />
  /// <exception cref="InvalidOperationException"></exception>
  public void Build() {
    if(_current == null)
      throw new InvalidOperationException("No trigger has been created. Use the appropriate method to create a trigger before building.");

    _current.Task = _task;
    _current.TaskId = _task.Id;
    _current.NextExecution = _current.GetNextExecution() ?? _current.StartsAt;
    _task.Triggers.Add(_current);
    _current = null;
  }

  /// <inheritdoc />
  /// <exception cref="InvalidOperationException"></exception>
  [Obsolete("This method is obsolete, use CreateIntervalTrigger with DateTimeOffset instead.")]
  public IIntervalTriggerBuilder CreateIntervalTrigger(DateTime startDate) => throw new InvalidOperationException("this method is obsolete, use CreateIntervalTrigger with DateTimeOffset instead");

  /// <inheritdoc />
  [Obsolete("This method is obsolete, use CreateCronTrigger with DateTimeOffset instead.")]
  public ICronExpressionTriggerStep CreateCronTrigger(DateTime startDate) => throw new InvalidOperationException("this method is obsolete, use CreateCronTrigger with DateTimeOffset instead");

  /// <inheritdoc />
  [Obsolete("This method is obsolete, use CreateCalendarTrigger with DateTimeOffset instead.")]
  public ICalendarTriggerBuilder CreateCalendarTrigger(DateTime startDate) => throw new InvalidOperationException("this method is obsolete, use CreateCalendarTrigger with DateTimeOffset instead");

  /// <inheritdoc />
  [Obsolete("This method is obsolete, use CreateDailyTrigger with DateTimeOffset instead.")]
  public IDailyTriggerBuilder CreateDailyTrigger(DateTime startDate) => throw new InvalidOperationException("this method is obsolete, use CreateDailyTrigger with DateTimeOffset instead");

  /// <inheritdoc />
  [Obsolete("This method is obsolete, use CreateWeeklyTrigger with DateTimeOffset instead.")]
  public IWeeklyTriggerBuild CreateWeeklyTrigger(DateTime startDate) => throw new InvalidOperationException("this method is obsolete, use CreateWeeklyTrigger with DateTimeOffset instead");

  /// <inheritdoc />
  [Obsolete("This method is obsolete, use CreateMonthlyTrigger with DateTimeOffset instead.")]
  public IMonthlyTriggerBuilder CreateMonthlyTrigger(DateTime startDate) => throw new InvalidOperationException("this method is obsolete, use CreateMonthlyTrigger with DateTimeOffset instead");
}
