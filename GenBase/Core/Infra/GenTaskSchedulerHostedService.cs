using GenTaskScheduler.Core.Abstractions.Common;
using Microsoft.Extensions.Hosting;

namespace GenTaskScheduler.Core.Infra;

///<inheritdoc />
/// <summary>
/// GenTaskSchedulerHostedService is a background service that runs the task scheduler.
/// </summary>
/// <param name="taskLauncher">Scheduler Launcher Instance</param>
public class GenTaskSchedulerHostedService(ISchedulerLauncher taskLauncher): BackgroundService {

  ///<inheritdoc />
  protected override Task ExecuteAsync(CancellationToken stoppingToken) => taskLauncher.ExecuteAsync(stoppingToken);
}

