using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;

namespace QBitTorrentSpeedScheduler.Service;

internal class TokenProvider : IDisposable
{
    private readonly TimeSpan _applyChangesCooldown = TimeSpan.FromSeconds(2);
    private readonly IOptionsMonitor<Settings> _optionsMonitor;
    private volatile CancellationTokenSource? _iterationCts;
    private IDisposable? _settingsChangeListener;
    private bool _disposed;

    public TokenProvider(IOptionsMonitor<Settings> optionsMonitor) => _optionsMonitor = optionsMonitor;

    public CancellationToken ScopedToken
    {
        get
        {
            ThrowIfDisposed();
            if (_iterationCts is null)
            {
                ThrowIfNotAttached();
            }
            return _iterationCts!.Token;
        }
    }

    public void AttachToWorker(CancellationToken globalStoppingToken)
    {
        if (globalStoppingToken == CancellationToken.None)
        {
            ThrowIfNotAttached();
        }
        ThrowIfDisposed();
        _iterationCts = CancellationTokenSource.CreateLinkedTokenSource(globalStoppingToken);
        var blocked = 0; // captured by OnChange delegate.
        _settingsChangeListener = _optionsMonitor.OnChange(
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

    public void Dispose()
    {
        _iterationCts?.Cancel();
        _iterationCts?.Dispose();
        _settingsChangeListener?.Dispose();
        _disposed = true;
    }

    private static void ThrowIfNotAttached() =>
        throw new InvalidOperationException(
            $"Need to register service worker token with {nameof(TokenProvider)}.{nameof(AttachToWorker)}.");

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
