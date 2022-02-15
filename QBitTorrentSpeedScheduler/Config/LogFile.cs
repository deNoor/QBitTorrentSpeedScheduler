namespace QBitTorrentSpeedScheduler.Config
{
    internal partial class LogFile
    {
        public bool Enabled { get; set; }
        public string? Folder { get; set; }
        public bool ErrorsOnly { get; set; }
    }

    internal partial class LogFile
    {
        internal static string FileName { get; } = $"{nameof(QBitTorrentSpeedScheduler)}.log";
    }
}