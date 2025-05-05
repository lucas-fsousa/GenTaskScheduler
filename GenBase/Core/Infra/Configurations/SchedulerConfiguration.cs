using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Infra.Configurations;

/// <summary>
/// Configuration class for the task scheduler.
/// Allows customizing behaviors such as retries on failure, 
/// delay between retries, and the margin of error for task execution.
/// </summary>
public class SchedulerConfiguration {
  private TimeSpan _databaseCheckInterval = TimeSpan.FromSeconds(30);
  
  /// <summary>
  /// Determines whether the scheduler should attempt to re-execute the task in case of failure. The default value is false.
  /// </summary>
  public bool RetryOnFailure { get; set; } = false;

  /// <summary>
  /// Defines the wait time between retry attempts in case of task execution failure.
  /// Used only when <see cref="RetryOnFailure"/> is true.
  /// </summary>
  public TimeSpan RetryWaitDelay { get; set; } = TimeSpan.FromSeconds(15);

  /// <summary>
  /// Specifies the maximum number of retry attempts before the task is permanently considered failed.
  /// Used only when <see cref="RetryOnFailure"/> is true.
  /// </summary>
  public int MaxRetry { get; set; } = 3;

  /// <summary>
  /// Defines the maximum allowed delay for a trigger to still be considered valid for execution.
  /// This tolerance is only applied to late executions, allowing the system to process a trigger
  /// slightly after its scheduled time in case of minor delays (e.g., processing backlog or clock drift).
  /// It does not permit early executions or affect the calculated next execution time.
  /// </summary>
  public TimeSpan LateExecutionTolerance { get; set; } = TimeSpan.FromMinutes(1);

  /// <summary>
  /// Defines the interval at which the database is checked for new or modified tasks.
  /// This controls how often the system checks the database for changes, typically to identify tasks to execute. the default value is 30 seconds (max 30 seconds).
  /// </summary>
  public TimeSpan DatabaseCheckInterval {
    get {
      return _databaseCheckInterval;
    }
    set {
      if(value > TimeSpan.FromSeconds(30))
        throw new ArgumentOutOfRangeException(nameof(value), "DatabaseCheckInterval cannot be greater than 30 seconds.");

      _databaseCheckInterval = value;
    }
  }

  /// <summary>
  /// Specifies the maximum number of scheduler tasks that can be executed in parallel.
  /// </summary>
  public int MaxTasksDegreeOfParallelism { get; set; } = 5;

  /// <summary>
  /// Indicates whether logging is enabled for the scheduler.
  /// </summary>
  public bool EnableLogging { get; set; } = true;

  /// <summary>
  /// Minimum log level to be used by the scheduler logger.
  /// </summary>
  public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
}

