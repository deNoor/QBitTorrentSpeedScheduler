using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;
using QBitTorrentSpeedScheduler.Helper;

namespace QBitTorrentSpeedScheduler.Service
{
    internal class Iteration
    {
        private readonly Settings _settings;
        private readonly GlobalLogChannel _logChannel;
        private readonly CancellationToken _token;

        private readonly SpeedConverter _converter;
        private readonly WebUiApi _api;

        public Iteration(
            IOptionsMonitor<Settings> settings,
            GlobalLogChannel logChannel,
            ScopedToken scopedToken,
            SpeedConverter converter,
            WebUiApi api)
        {
            _settings = settings.CurrentValue;
            _logChannel = logChannel;
            _token = scopedToken.Value;
            _converter = converter;
            _api = api;
        }

        public async Task<Task> DoAsync()
        {
            if (_settings.NothingToDo(out var reason))
            {
                _logChannel.LogError(reason);
                return Task.Delay(Timeout.InfiniteTimeSpan, _token);
            }

            var (uploadMegaBits, until) = _settings.FindCurrentSpeed(DateTime.Now.TimeOfDay);
            var newUploadSpeed = _converter.BytesFromMegaBits(uploadMegaBits);
            await _api.ApplyToRegularLimitsAsync(async api => await api.SetUploadLimitAsync(newUploadSpeed, _token));
            var nextTime = RemainsUntilNextEvent(until);
            _logChannel.LogInfo($"new upload speed {uploadMegaBits} MBit/s, next run {until}");
            // make sure timer won't wake up a bit early because of negative deviations in precision.
            nextTime += TimeSpan.FromMilliseconds(50);
            return Task.Delay(nextTime, _token);
        }

        private static TimeSpan RemainsUntilNextEvent(TimeSpan nextTime)
        {
            var diff = nextTime - DateTime.Now.TimeOfDay;
            if (diff < TimeSpan.Zero)
            {
                diff = diff.Add(TimeSpan.FromHours(24));
            }
            return diff;
        }
    }

    internal static partial class Extensions
    {
        public static IServiceCollection AddIteration(this IServiceCollection services) =>
            services.AddScoped<Iteration>().AddScopedToken().AddSpeedConverter().AddWebUiApi();
    }
}