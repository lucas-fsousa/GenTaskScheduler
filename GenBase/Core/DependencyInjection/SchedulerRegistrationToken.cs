namespace GenTaskScheduler.Core.DependencyInjection;

public sealed class SchedulerRegistrationToken {
  internal SchedulerRegistrationToken() { }

  public static SchedulerRegistrationToken Create() => new();
}
