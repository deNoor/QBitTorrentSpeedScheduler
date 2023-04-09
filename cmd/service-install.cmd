cd /d %~dp0
cd ..
sc create "qBitTorrent speed scheduler" binpath="%cd%\QBitTorrentSpeedScheduler.exe" start=auto
net start "qBitTorrent speed scheduler"
pause