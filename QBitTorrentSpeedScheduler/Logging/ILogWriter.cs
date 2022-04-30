using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal interface ILogWriter
{
    Task WriteAsync(LogLevel logLevel, string message, Exception? exception);
}