using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

public interface ITriggerFinisheStep {
  ITriggerBuilderStart Done();
  List<BaseTrigger> BuildAll();
  BaseTrigger Build();
}