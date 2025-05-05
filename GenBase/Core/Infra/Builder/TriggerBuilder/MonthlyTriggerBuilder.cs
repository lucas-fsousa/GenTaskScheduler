using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;
public partial class GenSchedulerTriggerBuilder: IMonthOfYearTriggerStep, IMonthlyTriggerBuilder {

  /// <inheritdoc/>
  public IMonthlyTriggerBuilder CreateMonthlyTrigger(DateTimeOffset startDate) {
    _current = new MonthlyTrigger();
    _current!.InternalSetStartDate(startDate);
    return this;
  }

  /// <inheritdoc/>
  public IMonthOfYearTriggerStep SetDaysOfMonth(params int[] daysOfMonth) {
    _current!.InternalSetDaysOfMonth(daysOfMonth);
    return this;
  }

  /// <inheritdoc/>
  public ICommonTriggerStep SetMonthsOfYear(params MonthOfYear[] monthOfYears) {
    _current!.InternalSetMonthsOfYear(monthOfYears);
    return this;
  }
}

