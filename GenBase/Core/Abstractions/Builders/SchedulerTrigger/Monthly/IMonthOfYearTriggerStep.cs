using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.SharedSteps;
using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger.Monthly;
public interface IMonthOfYearTriggerStep {
  ICommonTriggerStep SetMonthsOfYear(params MonthOfYear[] monthOfYears);
}

