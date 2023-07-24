# QBitTorrentSpeedScheduler
Windows service to control upload speed of [qBittorrent](https://www.qbittorrent.org/) using its [WebUI](https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)) (remote control).

Made for personal usage — feature requests or improvements are not accepted.\
No user interface, managed through a configuration file.\
Requires [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) runtime. Choose Desktop Runtime for Windows x64. Use `dotnet --info` to check if you already have it.

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
  - run as regular exe to use in console mode
- it will create a default `settings` file in the same folder if none is present
- edit and save settings to apply changes, app restart is not required

Controls QBitTorrent Regular limits only. You can switch to Alternative limits any time for manual speed control. 

Portable. Does not write anywhere (except the log file by the path you specified in settings, logging is disabled by default). If the log file is blocked from deletion, stop the Windows service first.

### Uninstall
Just unregister the Windows service using the script from `cmd` folder and delete the app folder.
