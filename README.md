# QBitTorrentSpeedScheduler
Windows service to control upload speed of [qBittorrent](https://www.qbittorrent.org/) using its [WebUI](https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)) (remote control).

Made for personal usage — feature requests or improvements are not accepted.

Requires [.NET 5](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) runtime. Choose Desktop Runtime for Windows x64. Use `dotnet --info` to check if you already have it.

### 1. Setup qBittorrent. 
- go to Settings → Web UI:
  - enable it
  - IP address: `localhost`, port: `22596` (choose any [random port](https://www.random.org/integers/?num=1&min=5001&max=49151&col=5&base=10&format=html&rnd=new))
  - enable `Bypass authentication for clients on localhost` (Required)
- apply
### 2. Install this scheduler as service or use as exe
- copy to a preferred location
  - use scripts from `cmd` to (un)install as Windows service
  - -or-
  - run as regular exe to use in console application mode
- it will create default `settings` file in the same folder if none is present
- edit and save settings to automatically apply, no restart is required

Controls Regular limits only. You can enable Alternative limit any time for manual speed control. 

Portable. Does not write anywhere (except the log file by the path you specified in settings, disabled by default). If the file is blocked from deletion stop Windows service first. To uninstall Windows service just unregister it using script from `cmd` folder.
