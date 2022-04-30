using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace QBitTorrentSpeedScheduler.Service;

internal class ScopedToken
{
    public ScopedToken(TokenProvider tokenProvider) => Value = tokenProvider.ScopedToken;
    public CancellationToken Value { get; }
}

internal static partial class Extensions
{
    public static IServiceCollection AddScopedToken(this IServiceCollection services) => services.AddScoped<ScopedToken>();
}
