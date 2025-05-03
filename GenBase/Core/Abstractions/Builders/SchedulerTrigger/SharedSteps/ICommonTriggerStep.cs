namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;

public interface ICommonTriggerStep : IFinisheTriggerStep {
  ICommonTriggerStep SetDescription(string description);
  ICommonTriggerStep SetValidity(DateTimeOffset? endsAt = null);
  ICommonTriggerStep SetAutoDelete(bool autoDelete);
  ICommonTriggerStep SetExecutionLimit(int? maxExecutions);
}
