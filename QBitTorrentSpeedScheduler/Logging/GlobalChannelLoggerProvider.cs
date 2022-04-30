using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal class GlobalChannelLoggerProvider : ILoggerProvider
{
    private readonly GlobalLogChannel _globalLogChannel;

    public GlobalChannelLoggerProvider(GlobalLogChannel globalLogChannel) => _globalLogChannel = globalLogChannel;

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => new GlobalChannelLogger(categoryName, _globalLogChannel);
}

internal static partial class Extensions
{
    private static IServiceCollection AddGlobalChannelLoggerProvider(this IServiceCollection services) =>
        services.AddSingleton<ILoggerProvider, GlobalChannelLoggerProvider>();
}

internal static partial class Extensions
{
    public static ILoggingBuilder AddGlobalLogChannel(this ILoggingBuilder builder) => AddGlobalLogChannel(builder, _ => { });

    public static ILoggingBuilder AddGlobalLogChannel(this ILoggingBuilder builder, Action<GlobalLogChannelOptions> configure)
    {
        builder.Services.AddGlobalChannelLoggerProvider()
           .AddGlobalLogChannel()
           .AddGlobalChannelLogFormatter(configure)
           .AddConsoleLogWriter()
           .AddFileLogWriter();
        return builder;
    }

    public static ILoggingBuilder LogMyCodeOnly(this ILoggingBuilder builder, bool enabled = true)
    {
        if (enabled)
        {
            builder.AddFilter("*", LogLevel.None).AddFilter(nameof(QBitTorrentSpeedScheduler), LogLevel.Trace);
        }
        return builder;
    }
}

internal class GlobalLogChannelOptions
{
    public bool IncludeTimestamp { get; set; } = true;
    public bool IncludeCategory { get; set; }
}

internal static partial class Extensions
{
    private static IServiceCollection AddGlobalChannelLogFormatter(
        this IServiceCollection services,
        Action<GlobalLogChannelOptions> configure) =>
        services.AddOptions<GlobalLogChannelOptions>().Configure(configure).Services;
}