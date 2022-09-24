namespace QBitTorrentSpeedScheduler.Config;

internal partial class Network
{
    public int Port { get; set; }
}

internal partial class Network
{
    public const int DefaultPort = 22596;

    public static Network Default { get; } = new() { Port = Network.DefaultPort, };
}
