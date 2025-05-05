using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

public partial class GenSchedulerTriggerBuilder: IDailyTriggerBuilder {

  /// <inheritdoc/>
  public IDailyTriggerBuilder CreateDailyTrigger(DateTimeOffset startDate) {
    _current = new DailyTrigger();
    _current!.InternalSetStartDate(startDate);
    return this;
  }

  /// <inheritdoc/>
  ICommonTriggerStep IDailyTriggerBuilder.SetTimeOfDay(TimeOnly time) {
    if(_current is DailyTrigger dt)
      dt.TimeOfDay = time;

    return this;
  }
}