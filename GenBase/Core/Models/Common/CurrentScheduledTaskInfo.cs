namespace GenTaskScheduler.Core.Models.Common;

/// <summary>
/// Represents the current scheduled task information.
/// </summary>
/// <param name="Task">Task representation</param>
/// <param name="TriggerId">Trigger ID associated with firing</param>
internal readonly record struct CurrentScheduledTaskInfo(ScheduledTask Task, Guid? TriggerId);

