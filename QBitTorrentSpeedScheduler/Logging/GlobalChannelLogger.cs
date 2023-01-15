using System;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal class GlobalChannelLogger : ILogger
{
    private readonly string _category;
    private readonly GlobalLogChannel _logChannel;

    public GlobalChannelLogger(string category, GlobalLogChannel logChannel)
    {
        _category = category;
        _logChannel = logChannel;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logChannel.Log(_category, logLevel, message, exception);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
}
