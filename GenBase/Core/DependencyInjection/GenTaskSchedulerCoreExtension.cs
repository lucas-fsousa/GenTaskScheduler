using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Services;
using GenTaskScheduler.Core.Infra;
using GenTaskScheduler.Core.Infra.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.DependencyInjection;
public static class GenTaskSchedulerCoreExtension {
  public static IServiceCollection AddGenTaskScheduler(this IServiceCollection services, SchedulerRegistrationToken? token) {
    if(token is null)
      throw new InvalidOperationException("Use a database-specific registration method to add the scheduler.");

    var config = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<SchedulerConfiguration>();
    services.AddLogging(logging => {
      logging.ClearProviders();
      logging.AddProvider(new SchedulerLoggerProvider(config));
      logging.SetMinimumLevel(LogLevel.Information);
    });

    services.AddScoped<ITaskRepository, TaskRepository>();
    services.AddScoped<ITriggerRepository, TriggerRepository>();
    services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
    services.AddSingleton<ISchedulerLauncher, SchedulerLauncher>();
    services.AddHostedService<GenTaskSchedulerHostedService>();
    return services;
  }
}
