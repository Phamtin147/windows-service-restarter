@echo off
title Tao Shortcut tren Desktop
cd /d "%~dp0"

echo Dang tao loi tat tren man hinh Desktop...

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ws = New-Object -ComObject WScript.Shell; " ^
  "$desktop = [Environment]::GetFolderPath('Desktop'); " ^
  "$s = $ws.CreateShortcut(\"$desktop\⚡ Fix Lag Windows.lnk\"); " ^
  "$s.TargetPath = '%~dp0WinLagFixer.exe'; " ^
  "$s.WorkingDirectory = '%~dp0'; " ^
  "$s.Description = 'Khac phuc giat lag & restart service Windows'; " ^
  "$s.IconLocation = '%SystemRoot%\System32\shell32.dll,238'; " ^
  "$s.Save()"

echo [V] Da tao thanh cong loi tat 'Fix Lag Windows' tren Desktop cua ban!
pause
