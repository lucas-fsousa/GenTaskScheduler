namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

public interface ITriggerCommonStep {
  ITriggerCommonStep WithDescription(string description);
  ITriggerCommonStep SetValidity(DateTimeOffset startsAt, DateTimeOffset? endsAt = null);
  ITriggerCommonStep SetAutoDelete(bool autoDelete);
  ITriggerCommonStep SetExecutionLimit(int? maxExecutions);
  ITriggerFinisheStep DoneCommon();
}
