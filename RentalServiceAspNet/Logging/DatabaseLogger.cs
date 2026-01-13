using Microsoft.Extensions.Logging;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Logging;

public class DatabaseLogger : ILogger
{
    private readonly string _categoryName;
    private readonly DatabaseLoggerProvider _provider;

    public DatabaseLogger(string categoryName, DatabaseLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var exceptionString = exception?.ToString();

        _provider.EnqueueLog(new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Category = _categoryName,
            Message = message,
            Exception = exceptionString
        });
    }
}
