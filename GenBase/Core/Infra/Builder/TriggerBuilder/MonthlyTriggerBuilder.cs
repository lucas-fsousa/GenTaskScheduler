using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;
using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Models.Common;
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
  public IMonthOfYearTriggerStep SetDaysOfMonth(params IntRange[] ranges) {
    if(ranges is null || ranges.Length == 0)
      throw new ArgumentNullException(nameof(ranges), "Ranges cannot be null or empty.");

    if(ranges.Any(r => r.Start <= 0 && r.End > 0)) {
      var invalidRange = ranges.First(r => r.Start <= 0 && r.End > 0);
      throw new ArgumentOutOfRangeException(nameof(ranges), $"Invalid range detected [{invalidRange.Start}-{invalidRange.End}]. Range must be greater than 0 if range is not IntRange.Zero");
    }
      

    var rangeExpands = ranges.SelectMany(x => x.Expand());
    _current!.InternalSetDaysOfMonth([..rangeExpands]);
    return this;
  }

  /// <inheritdoc/>
  public ITimerOfDayTriggerStep SetMonthsOfYear(params MonthOfYear[] monthOfYears) {
    _current!.InternalSetMonthsOfYear(monthOfYears);
    return this;
  }
}

