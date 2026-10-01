@echo off
title Khoi dong WinLagFixer
cd /d "%~dp0"

:: Kiem tra quyen Administrator
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] Dang yeu cau quyen Administrator...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~dp0WinLagFixer.exe' -Verb RunAs"
    exit /b
)

:: Neu da co quyen Admin thi chay luon .exe
start "" "%~dp0WinLagFixer.exe"
exit /b
