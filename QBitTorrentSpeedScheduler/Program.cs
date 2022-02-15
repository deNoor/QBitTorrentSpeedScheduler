using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QBitTorrentSpeedScheduler.Config;
using QBitTorrentSpeedScheduler.Service;

namespace QBitTorrentSpeedScheduler
{
    public class Program
    {
        public static async Task Main()
        {
            await AllConfig.InitFilesAsync();
            using var host = CreateHostBuilder(Array.Empty<string>()).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
               .UseWindowsService(options => options.ServiceName = "qBitTorrent speed scheduler")
               .ConfigureLogging(builder => builder.ClearProviders())
               .ConfigureAppConfiguration(builder => builder.AddAllConfig())
               .ConfigureServices(
                    (hostContext, services) => services.AddAllConfig(hostContext.Configuration).AddHostedWorker())
               .UseDefaultServiceProvider(
                    options =>
                    {
                        options.ValidateOnBuild = true;
                        options.ValidateScopes = true;
                    });
    }
}