using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Models.Triggers;
public class CronTrigger: BaseTrigger {
  /// <summary>
  /// Cron expression that defines the schedule for the task.
  /// </summary>
  public string CronExpression { get; set; } = null!;
}

