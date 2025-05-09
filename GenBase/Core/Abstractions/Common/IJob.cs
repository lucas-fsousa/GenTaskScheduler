namespace GenTaskScheduler.Core.Abstractions.Common;
public interface IJob {
  /// <summary>
  /// Executes the job logic.
  /// </summary>
  /// <param name="cancellationToken">The cancellation token to observe.</param>
  public Task ExecuteJobAsync(CancellationToken cancellationToken = default);
}

