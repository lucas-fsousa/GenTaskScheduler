using GenTaskScheduler.Core.Abstractions.Builders.SchedulerTrigger;
using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Infra.Builder.TriggerBuilder;

public partial class TriggerBuilder:
  ITriggerBuilderStart,
  ITriggerCommonStep,
  ITriggerFinisheStep {
  internal readonly List<BaseTrigger> _triggers = [];
  internal BaseTrigger? _current;
  private TriggerBuilder() { }

  public static ITriggerBuilderStart Start() => new TriggerBuilder();

  public ITriggerCommonStep OnceTrigger(DateTimeOffset executionTime) {
    _current = new OnceTrigger {
      StartsAt = executionTime,
      NextExecution = executionTime
    };
    return this;
  }

  public ITriggerCommonStep IntervalTrigger() {
    _current = new IntervalTrigger();
    return this;
  }

  public ITriggerCommonStep SetValidity(DateTimeOffset startsAt, DateTimeOffset? endsAt = null) {
    _current!.InternalSetValidity(startsAt, endsAt);
    return this;
  }

  public ITriggerCommonStep SetAutoDelete(bool autoDelete) {
    _current!.InternalSetAutoDelete(autoDelete);
    return this;
  }

  public ITriggerCommonStep WithDescription(string description) {
    _current!.InternalSetDescription(description);
    return this;
  }

  public ITriggerCommonStep SetExecutionLimit(int? maxExecutions) {
    _current!.InternalSetMaxExecutionLimit(maxExecutions);
    return this;
  }

  public ITriggerFinisheStep DoneCommon() => this;

  public ITriggerBuilderStart Done() {
    _triggers.Add(_current!);
    _current = null;
    return this;
  }

  public List<BaseTrigger> BuildAll() {
    if(_current != null)
      _triggers.Add(_current);

    return _triggers;
  }

  public BaseTrigger Build() {
    if(_current == null)
      throw new InvalidOperationException("No trigger has been created. Use the appropriate method to create a trigger before building.");

    var trigger = _current;
    _current = null;
    return trigger;
  }

  public ICalendarTriggerBuilder CreateCalendarTrigger() {
    _current = new CalendarTrigger();
    return this;
  }

  public ICronTriggerBuilder CreateCronTrigger() {
    _current = new CronTrigger();
    return this;
  }

  public IDailyTriggerBuilder CreateDailyTrigger() {
    _current = new DailyTrigger();
    return this;
  }

  public IIntervalTriggerBuilder CreateIntervalTrigger() {
    _current = new IntervalTrigger();
    return this;
  }

  public IMonthlyTriggerBuilder CreateMonthlyTrigger() {
    _current = new MonthlyTrigger();
    return this;
  }

  public IOnceTriggerBuilder CreateOnceTrigger() {
    _current = new OnceTrigger();
    return this;
  }

  public IWeeklyTriggerBuilder CreateWeeklyTrigger() {
    _current = new WeeklyTrigger();
    return this;
  }
}
