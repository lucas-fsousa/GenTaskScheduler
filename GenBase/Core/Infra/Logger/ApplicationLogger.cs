using GenTaskScheduler.Core.Infra.Configurations;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GenTaskScheduler.Core.Infra.Logger;

/// <summary>
/// ApplicationLogger is a custom logger implementation for the GenTaskScheduler application.
/// </summary>
/// <param name="categoryName">The category name for logger</param>
public class ApplicationLogger(string categoryName): ILogger {
  private static readonly SchedulerConfiguration _config = GenSchedulerEnvironment.SchedulerConfiguration;

  /// <inheritdoc/>
  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new SchedulerLogScope(state);

  /// <inheritdoc/>
  public bool IsEnabled(LogLevel logLevel) => _config.EnableLogging && logLevel >= _config.MinimumLogLevel;

  /// <inheritdoc/>
  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    if(!IsEnabled(logLevel))
      return;

    var originalColor = Console.ForegroundColor;
    var scopePrefix = GetScopeInfo();
    scopePrefix = string.IsNullOrEmpty(scopePrefix) ? "" : $"<{scopePrefix}>";

    Console.Write('[');
    Console.ForegroundColor = GetColor(logLevel);
    Console.Write(logLevel.ToString().ToUpper());
    Console.ForegroundColor = originalColor;
    Console.Write($" {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}]: {categoryName}{scopePrefix}\n\t{formatter(state, exception)}");
    Console.WriteLine(exception is null? "" : $"\n\t{FormatException(exception)}");
  }

  private static ConsoleColor GetColor(LogLevel level) => level switch {
    LogLevel.Critical => ConsoleColor.DarkRed,
    LogLevel.Error => ConsoleColor.Red,
    LogLevel.Warning => ConsoleColor.Yellow,
    LogLevel.Information => ConsoleColor.DarkGreen,
    LogLevel.Debug => ConsoleColor.Gray,
    LogLevel.Trace => ConsoleColor.DarkGray,
    _ => ConsoleColor.White,
  };

  private static string FormatException(Exception? exception) {
    if(exception is null)
      return string.Empty;
    
    var result = new StringBuilder();
    var level = 0;
    while(exception != null) {
      var tabs = new string('\t', level);
      result.AppendLine($"{tabs}{exception.GetType().Name} -> {exception.Message} {exception.StackTrace}");

      exception = exception.InnerException;
    }

    return result.ToString();
  }

  private static string GetScopeInfo() {
    var scope = SchedulerLogScope.Current;
    if(scope == null)
      return "";

    var scopes = string.Join(" => ", scope.GetScopeStack());
    return scopes;
  }
}

