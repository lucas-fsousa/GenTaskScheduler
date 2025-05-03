namespace GenTaskScheduler.Core.Infra.Logger;

public class SchedulerLogScope: IDisposable {
  private static readonly AsyncLocal<SchedulerLogScope?> _current = new();

  public static SchedulerLogScope? Current => _current.Value;

  public object State { get; }
  public SchedulerLogScope? Parent { get; }

  public SchedulerLogScope(object state) {
    State = state;
    Parent = _current.Value;
    _current.Value = this;
  }

  public void Dispose() {
    GC.SuppressFinalize(this);
    _current.Value = Parent;
  }

  public IEnumerable<object> GetScopeStack() {
    var scope = this;
    while(scope != null) {
      yield return scope.State;
      scope = scope.Parent;
    }
  }
}
