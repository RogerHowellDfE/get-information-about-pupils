using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;

internal static class LoggerTestDoubles
{
    internal static InMemoryLogger<T> MockLogger<T>()
    {
        InMemoryLogger<T> mockLogger = new();
        return mockLogger;
    }
}

internal sealed class InMemoryLogger<T> : ILogger<T>
{
    public List<string> Logs { get; } = [];

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception,
        string> formatter)
    {
        Logs.Add(formatter(state, exception));
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }
}
