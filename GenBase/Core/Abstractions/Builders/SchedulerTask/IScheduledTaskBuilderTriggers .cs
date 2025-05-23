﻿using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;

/// <summary>
/// Interface for configuring triggers of a scheduled task.
/// </summary>
public interface IScheduledTaskBuilderTriggers {
  /// <summary>
  /// Starts configuration of triggers for the scheduled task.
  /// </summary>
  /// <param name="configure">Delegate to configure one or more triggers using the builder.</param>
  /// <returns><see cref="IScheduledTaskBuilderDependsOn"/></returns>
  IScheduledTaskBuilderDependsOn ConfigureTriggers(Action<ITriggerBuilderStart> configure);

  /// <summary>
  /// Add a new trigger for the current task
  /// </summary>
  /// <param name="trigger">A simple <see cref="BaseTrigger"/> to add in the current task</param>
  /// <returns><see cref="IScheduledTaskBuilderDependsOn"/></returns>
  IScheduledTaskBuilderDependsOn AddTrigger(BaseTrigger trigger);


  /// <summary>
  /// Add an array of trigger for the current task
  /// </summary>
  /// <param name="triggers">Array of <see cref="BaseTrigger"/> to add in the current task</param>
  /// <returns><see cref="IScheduledTaskBuilderDependsOn"/></returns>
  IScheduledTaskBuilderDependsOn AddTriggers(params BaseTrigger[] triggers);
}