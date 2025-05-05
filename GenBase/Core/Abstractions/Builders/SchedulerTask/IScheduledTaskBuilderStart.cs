using GenTaskScheduler.Core.Abstractions.Common;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;

/// <summary>
/// Interface for the initial step of the scheduled task builder.
/// </summary>
public interface IScheduledTaskBuilderStart {

  /// <summary>
  /// Defines the job to be executed by the scheduled task.
  /// </summary>
  /// <param name="job">An action to be performed derived from the implementation of <see cref="IJob"/> </param>
  /// <returns><see cref="IScheduledTaskBuilderTriggers"/></returns>
  IScheduledTaskBuilderTriggers WithJob(IJob job);
}

