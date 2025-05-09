using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenTaskScheduler.Core.Abstractions.Providers;

/// <summary>
/// Defines the contract for database-specific configuration and migration support
/// for the GenTaskScheduler infrastructure.
/// </summary>
public interface IGenTaskSchedulerDatabaseProvider {
  /// <summary>
  /// Gets the name of the database provider (e.g., "SqlServer", "MySql", "PostgreSql").
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Configures the Entity Framework Core DbContext with the specific provider options.
  /// This method should register the DbContext and bind it to GenTaskSchedulerDbContext.
  /// </summary>
  /// <param name="services">The service collection used for dependency injection.</param>
  /// <param name="connectionString">The connection string to the target database.</param>
  void ConfigureDbContext(IServiceCollection services, string connectionString);

  /// <summary>
  /// Registers database-specific infrastructure and services required for task scheduling,
  /// such as ISchemeProvider implementations.
  /// </summary>
  /// <param name="services">The service collection used for dependency injection.</param>
  void RegisterInfrastructure(IServiceCollection services);

  /// <summary>
  /// Applies any pending Entity Framework Core migrations to the target database.
  /// Should be called explicitly when auto-migration is enabled.
  /// </summary>
  /// <param name="provider">The service provider used to resolve required services.</param>
  void ApplyMigrations(IServiceProvider provider);
}
