using System.Collections.Concurrent;
using Serilog;
using Serilog.Events;
using Craft.Logging.Logger;
using Serilog.Core;

namespace Craft.Logging.Tests.Logger;

public class CraftLoggerTests
{
    [Fact]
    public void EnsureInitialized_SetsLoggerInstance()
    {
        // Act
        CraftLogger.EnsureInitialized();

        // Assert
        Assert.NotNull(Log.Logger);
    }

    [Fact]
    public void EnsureInitialized_LoggerHasMinimumLevelWarning()
    {
        // Arrange
        CraftLogger.EnsureInitialized();
        var testSink = new TestSink();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Sink(testSink)
            .CreateLogger();

        // Act
        Log.Logger.Information("Info");
        Log.Logger.Warning("Warn");
        Log.Logger.Error("Error");

        // Assert
        Assert.DoesNotContain(testSink.Events, e => e.Level == LogEventLevel.Information);
        Assert.Contains(testSink.Events, e => e.Level == LogEventLevel.Warning);
        Assert.Contains(testSink.Events, e => e.Level == LogEventLevel.Error);
    }

    [Fact]
    public void EnsureInitialized_DoesNotLogBelowWarning()
    {
        // Arrange
        CraftLogger.EnsureInitialized();
        var testSink = new TestSink();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Sink(testSink)
            .CreateLogger();

        // Act
        Log.Logger.Debug("Debug");
        Log.Logger.Verbose("Verbose");
        Log.Logger.Information("Info");

        // Assert
        Assert.Empty(testSink.Events);
    }

    [Fact]
    public void EnsureInitialized_CanBeCalledMultipleTimes()
    {
        // Act & Assert
        CraftLogger.EnsureInitialized();

        var ex = Record.Exception(() => CraftLogger.EnsureInitialized());

        Assert.Null(ex);
    }

    private class TestSink : ILogEventSink
    {
        public ConcurrentBag<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
