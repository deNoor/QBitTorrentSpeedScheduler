namespace QBitTorrentSpeedScheduler.Config;

internal partial class Constraints
{
    public int WaitOnStartSeconds { get; set; }
    public int MinUploadKiloBits { get; set; }
    public int RetryOnErrorMinutes { get; set; }
    public int MaxRetries { get; set; }
}

internal partial class Constraints
{
    public static Constraints Default { get; } = new()
    {
        WaitOnStartSeconds = 10, MinUploadKiloBits = 64, RetryOnErrorMinutes = 5, MaxRetries = 10,
    };
}