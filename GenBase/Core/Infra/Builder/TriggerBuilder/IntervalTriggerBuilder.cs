using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

public partial class GenSchedulerTriggerBuilder: IIntervalTriggerBuilder {

  ///<inheritdoc />
  public IIntervalTriggerBuilder CreateIntervalTrigger(DateTimeOffset startDate) {
    _current = new IntervalTrigger();
    _current!.InternalSetStartDate(startDate);
    return this;
  }

  ///<inheritdoc />
  ///<exception cref="ArgumentOutOfRangeException"></exception>
  public ICommonTriggerStep SetRepeatIntervalMinutes(int minutes) {
    if(minutes <= 0)
      throw new ArgumentOutOfRangeException(nameof(minutes), "Repeat interval must be greater than zero.");

    _current!.ExecutionInterval = TimeSpan.FromMinutes(minutes);
    return this;
  }
}
