using GenTaskScheduler.Core.Enums;

namespace GenTaskScheduler.Core.Infra.Configurations;
public static class GenSchedulerEnvironment {
  private static readonly object _lock = new();
  private static string? _databaseConnectionString;
  private static SchedulerConfiguration? _schedulerConfiguration;

  public static GenSchedulerEnvironmentMode Mode { get; set; } = DetectMode();

  private static GenSchedulerEnvironmentMode DetectMode() {
    if(AppDomain.CurrentDomain.FriendlyName.Equals("ef", StringComparison.InvariantCultureIgnoreCase))
      return GenSchedulerEnvironmentMode.DesignTime;

#if DEBUG
    return GenSchedulerEnvironmentMode.Development;
#else
    return GenSchedulerEnvironmentMode.Production;
#endif
  }

  public static string DatabaseConnectionString {
    get {
      if(_databaseConnectionString == null)
        throw new InvalidOperationException("DatabaseConnectionString was not initialized.");
      return _databaseConnectionString;
    }
  }

  public static SchedulerConfiguration SchedulerConfiguration {
    get {
      if(_schedulerConfiguration == null)
        throw new InvalidOperationException("SchedulerConfiguration was not initialized.");
      return _schedulerConfiguration;
    }
  }

  public static bool IsDesignTime => Mode == GenSchedulerEnvironmentMode.DesignTime;
  public static bool IsProduction => Mode == GenSchedulerEnvironmentMode.Production;
  public static bool IsDevelopment => Mode == GenSchedulerEnvironmentMode.Development;

  public static void Initialize(string connectionString, SchedulerConfiguration configuration) {
    lock(_lock) {
      if(_databaseConnectionString != null || _schedulerConfiguration != null)
        throw new InvalidOperationException("GenSchedulerEnvironment has already been initialized.");

      ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
      _databaseConnectionString = connectionString;
      _schedulerConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
  }
}
