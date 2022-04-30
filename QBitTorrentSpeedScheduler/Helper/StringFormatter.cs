using System;
using System.Collections.Generic;

namespace QBitTorrentSpeedScheduler.Helper;

internal static class StringFormatter
{
    public static string AsLines(this IEnumerable<string> lines) => string.Join(Environment.NewLine, lines);

    public static string CurrentTimestamp() => $@"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]";
}
