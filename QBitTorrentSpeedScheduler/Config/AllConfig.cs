using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    public static AllConfig Default { get; } = new() { Settings = Settings.Default, };

    public static async Task InitFileAsync()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, Settings.FileName);
        if (!File.Exists(filePath))
        {
            await WriteAsync(Default);
        }

        async Task WriteAsync(AllConfig value)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true);
            await JsonSerializer.SerializeAsync(fileStream, value, _jsonOptions);
        }
    }
}

internal static partial class Extensions
{
    public static IConfigurationBuilder AddAllConfig(this IConfigurationBuilder configurationBuilder) =>
        configurationBuilder.SetBasePath(AppContext.BaseDirectory).AddSetting();

    public static IServiceCollection AddAllConfig(this IServiceCollection serviceCollection, IConfiguration configuration) =>
        serviceCollection.AddSettings(configuration);
}
