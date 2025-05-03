using GenTaskScheduler.Core.Abstractions.Common;
using Microsoft.Extensions.Hosting;

namespace GenTaskScheduler.Core.Infra;
public class GenTaskSchedulerHostedService(ISchedulerLauncher taskLauncher): BackgroundService {
  protected override Task ExecuteAsync(CancellationToken stoppingToken) => taskLauncher.ExecuteAsync(stoppingToken);
}

