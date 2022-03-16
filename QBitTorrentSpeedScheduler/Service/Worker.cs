using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;

namespace QBitTorrentSpeedScheduler.Service
{
    internal class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IOptionsMonitor<Settings> _optionsMonitor;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GlobalLogChannel _logChannel;
        private readonly TokenProvider _tokenProvider;
        private int _failCount;

        public Worker(
            IHostApplicationLifetime appLifetime,
            IOptionsMonitor<Settings> optionsMonitor,
            IServiceScopeFactory scopeFactory,
            GlobalLogChannel logChannel,
            TokenProvider tokenProvider)
        {
            _appLifetime = appLifetime;
            _optionsMonitor = optionsMonitor;
            _scopeFactory = scopeFactory;
            _logChannel = logChannel;
            _tokenProvider = tokenProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logChannel.LogInfo("Starting the service.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logChannel.LogInfo("Shutting down the service.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _tokenProvider.AttachToWorker(stoppingToken);
            await DelayBeforeStart(stoppingToken);
            while (!stoppingToken.IsCancellationRequested && !MaxFailsCount())
            {
                try
                {
                    Task iterationCooldown;
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var iteration = scope.ServiceProvider.GetRequiredService<Iteration>();
                        iterationCooldown = await iteration.DoAsync();
                    }
                    _failCount = 0;
                    await iterationCooldown;
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    _failCount++;
                    var settings = _optionsMonitor.CurrentValue;
                    var delay = TimeSpan.FromMinutes(Math.Max(1, settings.Constraints!.RetryOnErrorMinutes));
                    _logChannel.LogError(e.Message);
                    if (!MaxFailsCount())
                    {
                        _logChannel.LogInfo(
                            $@"failed {_failCount} times, next retry {DateTime.Now.TimeOfDay.Add(delay):hh\:mm\:ss}");
                        await Task.Delay(delay, stoppingToken);
                    }
                    else
                    {
                        _logChannel.LogError($"failed {_failCount} times.");
                    }
                }
            }
            _appLifetime.StopApplication();
        }

        private bool MaxFailsCount()
            => _failCount >= (_optionsMonitor.CurrentValue.Constraints ?? Constraints.Default).MaxRetries;

        private async Task DelayBeforeStart(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(_optionsMonitor.CurrentValue.Constraints?.WaitOnStartSeconds ?? default);
            if (delay > TimeSpan.Zero)
            {
                _logChannel.LogInfo($@"Waiting until {DateTime.Now.TimeOfDay.Add(delay):hh\:mm\:ss}");
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    internal static partial class Extensions
    {
        public static IServiceCollection AddHostedWorker(this IServiceCollection services)
            => services.AddIteration().AddLogChannel().AddTokenProvider().AddHostedService<Worker>();
    }
}