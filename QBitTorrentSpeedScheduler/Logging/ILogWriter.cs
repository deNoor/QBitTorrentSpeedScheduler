using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal interface ILogWriter
{
    Task WriteAsync(string category, LogLevel logLevel, string message, Exception? exception);
}