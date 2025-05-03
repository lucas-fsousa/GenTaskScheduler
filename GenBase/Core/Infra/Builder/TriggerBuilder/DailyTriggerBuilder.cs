using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

public partial class TriggerBuilder: IDailyTriggerBuilder {
  IDailyTriggerBuilder IDailyTriggerBuilder.SetTimeOfDay(TimeSpan time) {
    if(_current is DailyTrigger dt)
      dt.TimeOfDay = time;

    return this;
  }
}