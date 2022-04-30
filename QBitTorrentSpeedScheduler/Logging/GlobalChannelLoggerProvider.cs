using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal class GlobalChannelLoggerProvider : ILoggerProvider
{
    private readonly GlobalChannelLogger _globalChannelLogger;

    public GlobalChannelLoggerProvider(GlobalChannelLogger globalChannelLogger) => _globalChannelLogger = globalChannelLogger;

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => _globalChannelLogger;
}

internal static partial class Extensions
{
    private static IServiceCollection AddGlobalChannelLoggerProvider(this IServiceCollection services) =>
        services.AddSingleton<ILoggerProvider, GlobalChannelLoggerProvider>();
}

internal static partial class Extensions
{
    public static ILoggingBuilder AddGlobalLogChannel(this ILoggingBuilder builder)
    {
        builder.Services.AddGlobalChannelLoggerProvider()
           .AddGlobalChannelLogger()
           .AddGlobalLogChannel()
           .AddConsoleLogWriter()
           .AddFileLogWriter();
        return builder;
    }
}