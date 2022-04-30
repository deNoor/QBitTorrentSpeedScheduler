using System;
using Microsoft.Extensions.DependencyInjection;

namespace QBitTorrentSpeedScheduler.Logging;

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
