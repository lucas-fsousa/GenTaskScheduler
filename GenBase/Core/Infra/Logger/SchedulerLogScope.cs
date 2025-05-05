namespace GenTaskScheduler.Core.Infra.Logger;

/// <summary>
/// Represents a log scope for the scheduler.
/// </summary>
public class SchedulerLogScope: IDisposable {
  private static readonly AsyncLocal<SchedulerLogScope?> _current = new();

  /// <summary>
  /// Gets the current log scope for the scheduler.
  /// </summary>
  public static SchedulerLogScope? Current => _current.Value;
  
  /// <summary>
  /// Gets the state associated with this log scope.
  /// </summary>
  public object State { get; }

  /// <summary>
  /// Gets the parent log scope for this log scope.
  /// </summary>
  public SchedulerLogScope? Parent { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="SchedulerLogScope"/> class with the specified state.
  /// </summary>
  /// <param name="state">Scope state for initialize</param>
  public SchedulerLogScope(object state) {
    State = state;
    Parent = _current.Value;
    _current.Value = this;
  }

  // <inheritdoc />
  public void Dispose() {
    GC.SuppressFinalize(this);
    _current.Value = Parent;
  }

  /// <summary>
  /// Gets the current log scope for the scheduler.
  /// </summary>
  /// <returns></returns>
  public IEnumerable<object> GetScopeStack() {
    var scope = this;
    while(scope != null) {
      yield return scope.State;
      scope = scope.Parent;
    }
  }
}
