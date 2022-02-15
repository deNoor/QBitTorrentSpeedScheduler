using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;

namespace QBitTorrentSpeedScheduler.Service
{
    internal class WebUiApi
    {
        private const int AlternativeLimitsFlag = 1;
        private readonly HttpClient _httpClient;
        private readonly GlobalLogChannel _logChannel;

        public WebUiApi(HttpClient httpClient, GlobalLogChannel logChannel)
        {
            _httpClient = httpClient;
            _logChannel = logChannel;
        }

        public async Task<int> GetSpeedLimitsModeAsync(CancellationToken token = default)
        {
            var response = await _httpClient.GetAsync("speedLimitsMode", token);
            LogResponseError(response);
            return await JsonSerializer.DeserializeAsync<int>(
                await response.Content.ReadAsStreamAsync(token),
                cancellationToken: token);
        }

        public async Task ToggleSpeedLimitsModeAsync(CancellationToken token = default)
        {
            var response = await _httpClient.GetAsync("toggleSpeedLimitsMode", token);
            LogResponseError(response);
        }

        public async Task SetUploadLimitAsync(int speedInBytes, CancellationToken token = default)
        {
            var response = await _httpClient.PostAsync(
                "setUploadLimit",
                new FormUrlEncodedContent(new Dictionary<string, string?> { { "limit", $"{speedInBytes}" } }!),
                token);
            LogResponseError(response);
        }

        public async Task ApplyToRegularLimitsAsync(Func<WebUiApi, Task> action)
        {
            await using var _ = await LimitModeSaver.CreateAsync(this);
            await action(this);
        }

        private void LogResponseError(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var logger = _logChannel;
                var _ = Task.Run(
                    async () => logger.LogError(
                        "interaction with WebUI failed.",
                        $"{(int)response.StatusCode} {response.StatusCode}",
                        $"{response.RequestMessage?.RequestUri}",
                        $"{await response.Content.ReadAsStringAsync()}"),
                    CancellationToken.None);
            }
            response.EnsureSuccessStatusCode();
        }

        private class LimitModeSaver : IAsyncDisposable
        {
            private readonly WebUiApi _api;
            private bool _needSwitchSpeedLimitsMode;

            private LimitModeSaver(WebUiApi api)
            {
                _api = api;
                _needSwitchSpeedLimitsMode = false;
            }

            public static async Task<IAsyncDisposable> CreateAsync(WebUiApi api)
            {
                var instance = new LimitModeSaver(api);
                var needSwitch = await instance._api.GetSpeedLimitsModeAsync() == AlternativeLimitsFlag;
                if (needSwitch)
                {
                    await api.ToggleSpeedLimitsModeAsync();
                }
                instance._needSwitchSpeedLimitsMode = needSwitch;
                return instance;
            }

            public async ValueTask DisposeAsync()
            {
                if (_needSwitchSpeedLimitsMode)
                {
                    await _api.ToggleSpeedLimitsModeAsync();
                }
                _needSwitchSpeedLimitsMode = false;
            }
        }
    }

    internal static partial class Extensions
    {
        public static IServiceCollection AddWebUiApi(this IServiceCollection services)
        {
            services.AddScoped<WebUiApi>();
            services.AddHttpClient<WebUiApi>(
                (sc, client) =>
                {
                    var port = sc.GetRequiredService<IOptionsMonitor<Settings>>().CurrentValue.Network?.Port
                        ?? Network.DefaultPort;
                    client.BaseAddress = new($"http://127.0.0.1:{port}/api/v2/transfer/");
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestVersion = HttpVersion.Version20;
                });
            return services;
        }
    }
}