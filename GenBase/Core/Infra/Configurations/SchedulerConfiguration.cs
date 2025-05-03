using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Infra.Configurations;

/// <summary>
/// Configuration class for the task scheduler.
/// Allows customizing behaviors such as retries on failure, 
/// delay between retries, and the margin of error for task execution.
/// </summary>
public class SchedulerConfiguration {
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
  /// Defines the margin of error for the task execution. If the task doesn't execute within this margin of time
  /// after the desired execution time, it will be considered missed.
  /// For example, if the task is scheduled to run at 11:30 PM and the margin of error is 5 minutes, 
  /// it will still be considered valid if executed up to 11:35 PM.
  /// </summary>
  public TimeSpan MarginOfError { get; set; } = TimeSpan.FromMinutes(1);

  /// <summary>
  /// Defines the interval at which the database is checked for new or modified tasks.
  /// This controls how often the system checks the database for changes, typically to identify tasks to execute. the default value is 30 seconds.
  /// </summary>
  public TimeSpan DatabaseCheckInterval { get; set; } = TimeSpan.FromSeconds(30);

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

