using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QBitTorrentSpeedScheduler.Helper;

namespace QBitTorrentSpeedScheduler.Logging;

internal class GlobalChannelLogger : ILogger
{
    private readonly GlobalLogChannel _logChannel;

    public GlobalChannelLogger(GlobalLogChannel logChannel) => _logChannel = logChannel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception).PrefixCurrentTimestamp();
        _logChannel.Log(logLevel, message, exception);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) => default!;
}

internal static partial class Extensions
{
    private static IServiceCollection AddGlobalChannelLogger(this IServiceCollection services) =>
        services.AddTransient<GlobalChannelLogger>();
}