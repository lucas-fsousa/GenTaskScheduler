using GenTaskScheduler.Core.Infra.Configurations;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Infra.Logger;

public class SchedulerLoggerProvider: ILoggerProvider {
  public ILogger CreateLogger(string categoryName) => new ApplicationLogger(categoryName);

  public void Dispose() { 
    GC.SuppressFinalize(this);
    GC.Collect();
  }
}
