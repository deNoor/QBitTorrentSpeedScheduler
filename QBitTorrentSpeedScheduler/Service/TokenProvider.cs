using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;

namespace QBitTorrentSpeedScheduler.Service;

internal class TokenProvider : IDisposable
{
    private readonly TimeSpan _applyChangesCooldown = TimeSpan.FromSeconds(2);
    private readonly IDisposable? _settingsChangeListener;
    private volatile CancellationTokenSource? _iterationCts;
    private bool _disposed;

    public TokenProvider(IHostApplicationLifetime applicationLifetime, IOptionsMonitor<Settings> optionsMonitor)
    {
        var globalStoppingToken = applicationLifetime.ApplicationStopping;
        _iterationCts = CancellationTokenSource.CreateLinkedTokenSource(globalStoppingToken);
        var blocked = 0; // captured by OnChange delegate.
        _settingsChangeListener = optionsMonitor.OnChange(
            _ =>
            {
                Task.Run(
                    async () =>
                    {
                        // mitigate well known multiple reports from FileSystemWatcher for single change.
                        if (Interlocked.CompareExchange(ref blocked, 1, 0) == 1)
                        {
                            return;
                        }
                        var previousCts = Interlocked.Exchange(
                            ref _iterationCts,
                            CancellationTokenSource.CreateLinkedTokenSource(globalStoppingToken));
                        await Task.Delay(_applyChangesCooldown, globalStoppingToken);
                        Volatile.Write(ref blocked, 0);
                        previousCts.Cancel();
                        previousCts.Dispose();
                    },
                    globalStoppingToken);
            });
    }

    public CancellationToken ScopedToken
    {
        get
        {
            ThrowIfDisposed();
            return _iterationCts!.Token;
        }
    }

    public void Dispose()
    {
        var iterationCts = _iterationCts;
        iterationCts?.Cancel();
        iterationCts?.Dispose();
        _settingsChangeListener?.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TokenProvider));
        }
    }
}

internal static partial class Extensions
{
    public static IServiceCollection AddTokenProvider(this IServiceCollection services) => services.AddSingleton<TokenProvider>();
}
