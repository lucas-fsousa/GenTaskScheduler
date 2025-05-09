using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Infra.Logger;

/// <summary>
/// Provides a logger for the GenScheduler application.
/// </summary>
public class SchedulerLoggerProvider: ILoggerProvider {

  ///<inheritdoc />
  public ILogger CreateLogger(string categoryName) => new ApplicationLogger(categoryName);

  ///<inheritdoc />
  public void Dispose() { 
    GC.SuppressFinalize(this);
    GC.Collect();
  }
}
