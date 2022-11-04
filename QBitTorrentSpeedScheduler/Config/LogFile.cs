using System;

namespace QBitTorrentSpeedScheduler.Config;

internal partial class LogFile
{
    public bool Enabled { get; set; }
    public string? Folder { get; set; }
    public bool ErrorsOnly { get; set; }
}

internal partial class LogFile
{
    internal static string FileName { get; } = $"{nameof(QBitTorrentSpeedScheduler)}.log";

    public static LogFile Default { get; } = new LogFile { Enabled = false, Folder = AppContext.BaseDirectory, ErrorsOnly = false, }.Configure();

    internal LogFile Configure()
    {
        var folder = Folder;
        if (string.IsNullOrWhiteSpace(folder))
        {
            folder = AppContext.BaseDirectory;
        }
        Folder = folder;
        return this;
    }
}
