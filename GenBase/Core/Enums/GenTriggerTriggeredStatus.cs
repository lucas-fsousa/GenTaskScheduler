namespace GenTaskScheduler.Core.Enums;

/// <summary>
/// Represents the status of a trigger after it has been executed.
/// </summary>
public enum GenTriggerTriggeredStatus {
  NotTriggered,
  Success,
  Missfire
}