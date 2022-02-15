using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QBitTorrentSpeedScheduler.Config
{
    internal partial class RateLimitRule
    {
        [JsonConverter(typeof(ShortTimeConverter))]
        public TimeSpan Time { get; set; }
        public int UploadMegaBits { get; set; }
    }

    internal partial class RateLimitRule
    {
        public static readonly IComparer<RateLimitRule> TimeComparer =
            Comparer<RateLimitRule>.Create((x, y) => x.Time.CompareTo(y.Time));

        private static readonly TimeSpan _invalidSerializationTimeSpan = TimeSpan.MinValue;

        public bool IsValid() => Time != _invalidSerializationTimeSpan;

        private class ShortTimeConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (typeToConvert == typeof(TimeSpan))
                {
                    throw new InvalidOperationException(
                        $"Use {nameof(ShortTimeConverter)} for {nameof(TimeSpan)} only.");
                }
                return TimeSpan.TryParse(reader.GetString(), out var timeSpan)
                    ? timeSpan
                    : _invalidSerializationTimeSpan;
            }

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString(@"hh\:mm"));
        }
    }
}