namespace GenTaskScheduler.Core.Abstractions.Common;

/// <summary>
/// ISchedulerLauncher is an interface that defines the contract for a scheduler launcher.
/// </summary>
public interface ISchedulerLauncher {
  /// <summary>
  /// Executes the scheduler launcher.
  /// </summary>
  /// <param name="cancellationToken">Token for asynchronous operations</param>
  /// <returns></returns>
  public Task ExecuteAsync(CancellationToken cancellationToken = default);
}

