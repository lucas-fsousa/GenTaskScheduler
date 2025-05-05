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
  public ICommonTriggerStep SetRepeatIntervalHours(int hours) {
    if(hours <= 0)
      throw new ArgumentOutOfRangeException(nameof(hours), "Repeat interval in hours must be greater than zero.");

    if(hours > 23)
      throw new ArgumentOutOfRangeException(nameof(hours), $"Repeat interval in hours must be less than to 24 or use a {nameof(CreateDailyTrigger)} for daily executions");

    _current!.ExecutionInterval = TimeSpan.FromHours(hours);
    return this;
  }

  ///<inheritdoc />
  ///<exception cref="ArgumentOutOfRangeException"></exception>
  public ICommonTriggerStep SetRepeatIntervalMinutes(int minutes) {
    if(minutes <= 0)
      throw new ArgumentOutOfRangeException(nameof(minutes), "Repeat interval in minutes must be greater than zero.");

    if(minutes > 59)
      throw new ArgumentOutOfRangeException(nameof(minutes), "Repeat interval in minutes must be less than to 60");

    _current!.ExecutionInterval = TimeSpan.FromMinutes(minutes);
    return this;
  }
}
