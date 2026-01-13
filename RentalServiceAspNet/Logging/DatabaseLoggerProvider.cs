using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Models;
using System.Collections.Concurrent;

namespace RentalServiceAspNet.Logging;

public class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentQueue<LogEntry> _logQueue = new();
    private readonly Timer _flushTimer;
    private readonly int _batchSize = 50;
    private readonly object _lockObject = new();

    public DatabaseLoggerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _flushTimer = new Timer(FlushLogs, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DatabaseLogger(categoryName, this);
    }

    public void EnqueueLog(LogEntry logEntry)
    {
        _logQueue.Enqueue(logEntry);

        if (_logQueue.Count >= _batchSize)
        {
            Task.Run(() => FlushLogs(null));
        }
    }

    private void FlushLogs(object? state)
    {
        lock (_lockObject)
        {
            if (_logQueue.IsEmpty)
                return;

            using var scope = _serviceProvider.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var logsToSave = new List<LogEntry>();
                while (_logQueue.TryDequeue(out var log) && logsToSave.Count < _batchSize)
                {
                    logsToSave.Add(log);
                }

                if (logsToSave.Any())
                {
                    context.LogEntries.AddRange(logsToSave);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<DatabaseLoggerProvider>>();
                    logger?.LogError(ex, "Ошибка сохранения логов в БД");
                }
                catch
                {
                    Console.WriteLine($"Критическая ошибка сохранения логов в БД: {ex.Message}");
                }
            }
        }
    }

    public void Dispose()
    {
        _flushTimer?.Dispose();
        FlushLogs(null);
    }
}
