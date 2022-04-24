using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;
using static System.Math;

namespace QBitTorrentSpeedScheduler.Helper
{
    internal class SpeedConverter
    {
        private const int MinKiloBits = 8;
        private const int MaxKiloBits = 1024;

        private const int MaxMegaBits = 10_000;
        private const int UnlimitedValue = -1;

        private readonly int _minUploadSpeed;

        public SpeedConverter(IOptionsMonitor<Settings> settings) =>
            _minUploadSpeed = BytesFromKiloBits(settings.CurrentValue.Constraints!.MinUploadKiloBits);

        public int BytesFromMegaBits(int megaBits) =>
            UnlimitedMegaBits(megaBits) ? UnlimitedValue : Max(_minUploadSpeed, megaBits * 1_000_000 / 8);

        private static bool UnlimitedMegaBits(int megaBits) => megaBits is < 0 or >= MaxMegaBits;
        private static int BytesFromKiloBits(int kiloBits) => Max(MinKiloBits, Min(kiloBits, MaxKiloBits)) * 1000 / 8;
    }

    internal static class Extensions
    {
        public static IServiceCollection AddSpeedConverter(this IServiceCollection services) => services.AddTransient<SpeedConverter>();
    }
}