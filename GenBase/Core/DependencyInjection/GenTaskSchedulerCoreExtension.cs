using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Services;
using GenTaskScheduler.Core.Infra;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.DependencyInjection;
public static class GenTaskSchedulerCoreExtension {
  public static IServiceCollection AddGenTaskScheduler(this IServiceCollection services, SchedulerRegistrationToken? token) {
    if(token is null)
      throw new InvalidOperationException("Use a database-specific registration method to add the scheduler.");

    services.AddLogging(logging => {
      logging.ClearProviders();
      logging.AddProvider(new SchedulerLoggerProvider());
      logging.SetMinimumLevel(LogLevel.Information);
    });

    if(!GenSchedulerEnvironment.IsDesignTime) {
      services.AddScoped<ITaskRepository, TaskRepository>();
      services.AddScoped<ITriggerRepository, TriggerRepository>();
      services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
      services.AddSingleton<ISchedulerLauncher, GenSchedulerLauncher>();
      services.AddHostedService<GenTaskSchedulerHostedService>();
    }

    return services;
  }
}
