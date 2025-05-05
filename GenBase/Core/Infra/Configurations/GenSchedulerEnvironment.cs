using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Infra.Configurations;

/// <summary>
/// Represents the environment for the GenScheduler.
/// </summary>
public static class GenSchedulerEnvironment {
  private static readonly object _lock = new();
  private static string? _databaseConnectionString;
  private static SchedulerConfiguration? _schedulerConfiguration;

  /// <summary>
  /// Gets the current environment mode.
  /// </summary>
  public static GenSchedulerEnvironmentMode Mode { get; set; } = DetectMode();

  /// <summary>
  /// Gets the database connection string.
  /// </summary>
  /// <exception cref="InvalidOperationException"></exception>
  public static string DatabaseConnectionString {
    get {
      if(_databaseConnectionString == null)
        throw new InvalidOperationException("DatabaseConnectionString was not initialized.");
      
      return _databaseConnectionString;
    }
  }

  /// <summary>
  /// Gets the scheduler configuration.
  /// </summary>
  /// <exception cref="InvalidOperationException"></exception>
  public static SchedulerConfiguration SchedulerConfiguration {
    get {
      if(_schedulerConfiguration == null)
        throw new InvalidOperationException("SchedulerConfiguration was not initialized.");
      return _schedulerConfiguration;
    }
  }

  /// <summary>
  /// Checks if the current environment is design time.
  /// </summary>
  public static bool IsDesignTime => Mode == GenSchedulerEnvironmentMode.DesignTime;

  /// <summary>
  /// Checks if the current environment is production.
  /// </summary>
  public static bool IsProduction => Mode == GenSchedulerEnvironmentMode.Production;

  /// <summary>
  /// Checks if the current environment is development.
  /// </summary>
  public static bool IsDevelopment => Mode == GenSchedulerEnvironmentMode.Development;

  /// <summary>
  /// Initializes the GenScheduler environment with the specified database connection string and configuration.
  /// </summary>
  /// <param name="connectionString">Database connection string.</param>
  /// <param name="configuration">Schedule configuration</param>
  /// <exception cref="InvalidOperationException"></exception>
  /// <exception cref="ArgumentNullException"></exception>
  public static void Initialize(string connectionString, SchedulerConfiguration configuration) {
    lock(_lock) {
      if(_databaseConnectionString != null || _schedulerConfiguration != null)
        throw new InvalidOperationException("GenSchedulerEnvironment has already been initialized.");

      ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
      _databaseConnectionString = connectionString;
      _schedulerConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
  }

  private static GenSchedulerEnvironmentMode DetectMode() {
    if(AppDomain.CurrentDomain.FriendlyName.Equals("ef", StringComparison.InvariantCultureIgnoreCase))
      return GenSchedulerEnvironmentMode.DesignTime;

#if DEBUG
    return GenSchedulerEnvironmentMode.Development;
#else
    return GenSchedulerEnvironmentMode.Production;
#endif
  }
}
