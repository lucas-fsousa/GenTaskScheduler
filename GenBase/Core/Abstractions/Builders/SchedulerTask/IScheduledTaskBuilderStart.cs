using GenTaskScheduler.Core.Abstractions.Common;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTask;
public interface IScheduledTaskBuilderStart {
  IScheduledTaskBuilderTriggers WithJob(IJob job);
}

