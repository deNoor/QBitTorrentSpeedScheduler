using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QBitTorrentSpeedScheduler.Config;

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
        Network = Network.Default,
        LogFile = LogFile.Default,
        Schedule = new()
        {
            new() { Time = TimeSpan.Parse("01:00"), UploadMegaBits = 25, },
            new() { Time = TimeSpan.Parse("02:00"), UploadMegaBits = 300, },
            new() { Time = TimeSpan.Parse("08:00"), UploadMegaBits = 100, },
            new() { Time = TimeSpan.Parse("12:00"), UploadMegaBits = 50, },
            new() { Time = TimeSpan.Parse("16:00"), UploadMegaBits = 25, },
            new() { Time = TimeSpan.Parse("18:00"), UploadMegaBits = 0, },
        },
        Constraints = Constraints.Default,
    };

    private bool _validated;
    private bool _areInvalid;
    private string _errors = string.Empty;

    public bool AreInvalid(out string reason)
    {
        reason = _errors;
        if (_validated)
        {
            return _areInvalid;
        }
        var errors = new StringBuilder();
        errors.AppendLine();
        var areInvalid = false;
        if (!Schedule!.Any())
        {
            errors.AppendLine("schedule is empty - nothing to do.");
            areInvalid = true;
        }
        if (Network is null || Network.Port == default)
        {
            errors.AppendLine("webUI url not configured.");
            areInvalid = true;
        }
        _errors = errors.ToString();
        reason = _errors;
        _areInvalid = areInvalid;
        _validated = true;
        return _areInvalid;
    }

    public (int UploadMegaBits, TimeSpan Until) FindCurrentSpeed(TimeSpan now)
    {
        var list = Schedule!;
        var currentIndex = list.BinarySearch(new() { Time = now, }, RateLimitRule.TimeComparer);
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

    internal Settings Configure()
    {
        Network ??= Network.Default;
        (LogFile ??= LogFile.Default).Configure();
        Schedule = Schedule?.Where(x => x.IsValid()).OrderBy(x => x.Time).ToList() ?? new List<RateLimitRule>();
        Constraints ??= Constraints.Default;
        return this;
    }
}

internal static partial class Extensions
{
    public static IConfigurationBuilder AddSetting(this IConfigurationBuilder configurationBuilder) =>
        configurationBuilder.AddJsonFile(Settings.FileName, false, true);

    public static IServiceCollection AddSettings(this IServiceCollection serviceCollection, IConfiguration configuration) =>
        serviceCollection.AddOptions<Settings>()
           .Bind(configuration.GetSection(nameof(Settings)))
           .Configure(settings => settings.Configure())
           .Services;
}
