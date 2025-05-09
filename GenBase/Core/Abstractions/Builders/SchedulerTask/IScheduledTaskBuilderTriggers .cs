using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

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
}