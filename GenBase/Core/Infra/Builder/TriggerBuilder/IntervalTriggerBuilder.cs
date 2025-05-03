using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

public partial class TriggerBuilder: IIntervalTriggerBuilder {
  public IIntervalTriggerBuilder SetInitialExecution(DateTimeOffset initialTime) {
    if(initialTime < DateTimeOffset.UtcNow)
      throw new ArgumentOutOfRangeException(nameof(initialTime), "Initial execution time cannot be in the past.");
    
    _current!.StartsAt = initialTime;
    return this;
  }

  public IIntervalTriggerBuilder SetRepeatIntervalMinutes(int minutes) {
    if(minutes <= 0)
      throw new ArgumentOutOfRangeException(nameof(minutes), "Repeat interval must be greater than zero.");

    _current!.ExecutionInterval = TimeSpan.FromMinutes(minutes);
    return this;
  }
}
