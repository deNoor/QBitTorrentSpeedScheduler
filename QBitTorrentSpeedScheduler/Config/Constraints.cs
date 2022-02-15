namespace QBitTorrentSpeedScheduler.Config
{
    internal partial class Constraints
    {
        public int MinUploadKiloBits { get; set; }
        public int RetryOnErrorMinutes { get; set; }
        public int MaxRetries { get; set; }
    }

    internal partial class Constraints
    {
        public static Constraints Default { get; } = new()
        {
            MinUploadKiloBits = 64, RetryOnErrorMinutes = 5, MaxRetries = 10,
        };
    }
}