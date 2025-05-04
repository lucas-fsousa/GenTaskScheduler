namespace GenTaskScheduler.Core.Models.Common;
internal readonly record struct CurrentScheduledTaskInfo(ScheduledTask Task, Guid? TriggerId);

