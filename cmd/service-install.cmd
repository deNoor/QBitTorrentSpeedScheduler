cd /d %~dp0
cd ..
sc create "qBitTorrent speed scheduler" binpath="%cd%\QBitTorrentSpeedScheduler.exe" start=auto obj="NT AUTHORITY\Local Service" password=""
net start "qBitTorrent speed scheduler"
pause