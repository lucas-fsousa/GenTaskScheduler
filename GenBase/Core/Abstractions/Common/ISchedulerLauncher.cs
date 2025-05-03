namespace GenTaskScheduler.Core.Abstractions.Common;
public interface ISchedulerLauncher {

  public Task ExecuteAsync(CancellationToken cancellationToken = default);
}

