using GenTaskScheduler.Core.Abstractions.Common;
using GenTaskScheduler.Core.Abstractions.Providers;
using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Services;
using GenTaskScheduler.Core.Infra;
using GenTaskScheduler.Core.Infra.Configurations;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace GenTaskScheduler.Core.DependencyInjection;

public static class GenTaskSchedulerCoreExtension {

  /// <summary>
  /// Registers core GenTaskScheduler services.
  /// </summary>
  /// <remarks>
  /// This method should not be used directly.  
  /// Instead, use one of the provider-specific methods like:  
  /// <c>AddGenTaskSchedulerWithSqlServer(...)</c>,  
  /// <c>AddGenTaskSchedulerWithMySql(...)</c>, etc.  
  /// These methods ensure proper configuration for each database type.
  /// </remarks>
  /// <param name="services">The DI service collection.</param>
  /// <param name="token">The internal registration token used to guard usage.</param>
  /// <returns>The original service collection with scheduler services registered.</returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the provided token is null.  
  /// This indicates misuse; the scheduler should be registered through a database-specific method.
  /// </exception>
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

  /// <summary>
  /// Registers the GenTaskScheduler services using a custom database provider.
  /// </summary>
  /// <typeparam name="TProvider">
  /// The concrete implementation of <see cref="IGenTaskSchedulerDatabaseProvider"/> responsible for configuring 
  /// Entity Framework Core and additional infrastructure services specific to the database engine.
  /// </typeparam>
  /// <param name="services">The dependency injection service collection.</param>
  /// <param name="connectionString">The connection string used by the scheduler to access the database.</param>
  /// <param name="setup">
  /// An optional delegate to configure <see cref="SchedulerConfiguration"/> used by the scheduler engine.
  /// </param>
  /// <param name="applyMigrations">
  /// Whether to automatically apply pending Entity Framework Core migrations at startup. 
  /// Defaults to <c>false</c>. Should be used with caution in production environments.
  /// </param>
  /// <returns>The modified <see cref="IServiceCollection"/> instance.</returns>
  /// <remarks>
  /// This method is intended for use by consumers who want to register the scheduler with a database 
  /// other than SQL Server, or with custom database configurations. 
  /// Implement a custom <see cref="IGenTaskSchedulerDatabaseProvider"/> to control how the DbContext is registered 
  /// and how schema/migrations are handled.
  /// </remarks>
  public static IServiceCollection AddGenTaskSchedulerWithProvider<TProvider>(this IServiceCollection services, string connectionString, Action<SchedulerConfiguration>? setup = null, bool applyMigrations = false) where TProvider : class, IGenTaskSchedulerDatabaseProvider, new() {
    var config = new SchedulerConfiguration();
    setup?.Invoke(config);

    GenSchedulerEnvironment.Initialize(connectionString, config);

    services.AddGenTaskScheduler(SchedulerRegistrationToken.Create());

    var sqlProviderInstance = new TProvider();
    sqlProviderInstance.ConfigureDbContext(services, connectionString);
    sqlProviderInstance.RegisterInfrastructure(services);

    // Optional: auto apply migrations
    if(applyMigrations) {
      using var provider = services.BuildServiceProvider();
      var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger($"{Assembly.GetExecutingAssembly().GetName().Name!}[MIGRATION WARNING]");

      logger.LogWarning("""
        ⚠️ WARNING: Automatic EF Core migrations are enabled (applyMigrations = true).
        This may result in unexpected schema changes in production environments.

        ✅ Recommendation:
           Set applyMigrations = false in production environments and use the SchemeExporter 
           utility to generate the SQL scripts for manual review and execution by your DBA.

        Example usage:
            var schemeProvider = provider.GetRequiredService<ISchemeProvider>();
            var scripts = schemeProvider.GenerateSchemeScript(); // get string scripts
            // or save as sql file
            File.AppendAllText("/your/path/for/file.sql", scripts);
        """);

      logger.LogInformation("Migrations will be applied in 5 seconds");
      Task.WaitAny(Task.Delay(TimeSpan.FromSeconds(5)));
      sqlProviderInstance.ApplyMigrations(provider);
    }

    return services;
  }

}
