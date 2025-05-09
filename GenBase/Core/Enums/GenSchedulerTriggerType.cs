namespace GenTaskScheduler.Core.Enums;

/// <summary>
/// Enum representing the different types of triggers that can be used in the GenScheduler.
/// </summary>
public enum GenSchedulerTriggerType {
  Cron,
  Interval,
  OneTime,
  Daily,
  Weekly,
  Monthly,
  Yearly
}

