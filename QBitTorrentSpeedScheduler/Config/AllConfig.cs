using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QBitTorrentSpeedScheduler.Config;

internal partial class AllConfig
{
    public Settings? Settings { get; set; }
}

internal partial class AllConfig
{
    public static AllConfig Default { get; } = new() { Settings = Settings.Default, };

    public static async Task InitFilesAsync() => await Settings.InitFileAsync();
}

internal static partial class Extensions
{
    public static IConfigurationBuilder AddAllConfig(this IConfigurationBuilder configurationBuilder) =>
        configurationBuilder.SetBasePath(AppContext.BaseDirectory).AddSetting();

    public static IServiceCollection AddAllConfig(this IServiceCollection serviceCollection, IConfiguration configuration) =>
        serviceCollection.AddSettings(configuration);
}