using Cronos;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class TriggerBuilder: ICronTriggerBuilder {
  public ICronTriggerBuilder SetCronExpression(string expression) {
    if(string.IsNullOrWhiteSpace(expression))
      throw new ArgumentException("Cron expression cannot be null or empty", nameof(expression));

    if(!CronExpression.TryParse(expression, out _))
      throw new ArgumentException("Invalid cron expression", nameof(expression));

    if(_current is CronTrigger ct)
      ct.CronExpression = expression;
    else
      throw new InvalidOperationException("Current trigger is not a CronTrigger.");

    return this;
  }
}

