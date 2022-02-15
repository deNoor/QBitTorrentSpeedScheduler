using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QBitTorrentSpeedScheduler.Config
{
    internal partial class Settings
    {
        public Network? Network { get; set; }
        public LogFile? LogFile { get; set; }
        public List<RateLimitRule>? Schedule { get; set; }
        public Constraints? Constraints { get; set; }
    }

    internal partial class Settings
    {
        public const string FileName = "settings.json";

        internal static readonly Settings Default = new()
        {
            Network = new() { Port = Network.DefaultPort },
            LogFile = new() { Enabled = false, Folder = AppContext.BaseDirectory, ErrorsOnly = false },
            Schedule = new()
            {
                new() { Time = TimeSpan.Parse("01:00"), UploadMegaBits = 25 },
                new() { Time = TimeSpan.Parse("02:00"), UploadMegaBits = 300 },
                new() { Time = TimeSpan.Parse("08:00"), UploadMegaBits = 100 },
                new() { Time = TimeSpan.Parse("12:00"), UploadMegaBits = 50 },
                new() { Time = TimeSpan.Parse("16:00"), UploadMegaBits = 25 },
                new() { Time = TimeSpan.Parse("18:00"), UploadMegaBits = 0 },
            },
            Constraints = Constraints.Default,
        };

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        public static async Task InitFileAsync()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(filePath))
            {
                await using var fileStream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    true);
                await JsonSerializer.SerializeAsync(fileStream, AllConfig.Default, _jsonOptions);
            }
        }

        public bool NothingToDo(out string reason)
        {
            reason = string.Empty;
            if (!Schedule!.Any())
            {
                reason = "schedule is empty - nothing to do.";
                return true;
            }
            if (Network is null || Network.Port == default)
            {
                reason = "webUI url not configured - nothing to do.";
                return true;
            }
            return false;
        }

        public (int UploadMegaBits, TimeSpan Until) FindCurrentSpeed(TimeSpan now)
        {
            var list = Schedule!;
            var currentIndex = list.BinarySearch(new() { Time = now }, RateLimitRule.TimeComparer);
            if (currentIndex < 0)
            {
                currentIndex = ~currentIndex - 1;
                if (currentIndex < 0)
                {
                    currentIndex = list.Count - 1;
                }
            }
            var nextIndex = (currentIndex + 1) % list.Count;
            return (list[currentIndex].UploadMegaBits, list[nextIndex].Time);
        }
    }

    internal static partial class Extensions
    {
        public static IConfigurationBuilder AddSetting(this IConfigurationBuilder configurationBuilder)
            => configurationBuilder.AddJsonFile(Settings.FileName, false, true);

        public static IServiceCollection AddSettings(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.AddOptions<Settings>()
               .Bind(configuration.GetSection(nameof(Settings)))
               .Configure(
                    settings =>
                    {
                        settings.Schedule =
                            settings.Schedule?.Where(x => x.IsValid())
                               .OrderBy(x => x.Time)
                               .ToList()
                            ?? new List<RateLimitRule>();
                        settings.Constraints ??= Constraints.Default;
                    });
            return serviceCollection;
        }
    }
}