# ⚡ Windows Service Restarter & Anti-Lag Tool

A lightweight, standalone Windows utility designed to instantly eliminate micro-stutters, resolve desktop/taskbar freezing, reload GPU display drivers, flush standby RAM cache, and safely restart problematic Windows background services **without closing user applications or interrupting work sessions**.

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-blue.svg)](https://microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Runtime](https://img.shields.io/badge/.NET-4.5%2B%20(Native)-purple.svg)](https://dotnet.microsoft.com/)

---

## 🌟 Key Features

- **⚡ 1-Click Quick Fix (Khắc Phục Nhanh):**
  - **GPU Driver Pipeline Refresh:** Triggers the native Windows graphics subsystem reload sequence (`Win + Ctrl + Shift + B`) to resolve GPU driver hangs and display freezes without killing running applications.
  - **Windows Explorer Restart:** Refreshes `explorer.exe` (Taskbar, Desktop, Start Menu) and clears shell memory leaks.
  - **Windows Audio Restart:** Restarts `AudioEndpointBuilder` and `AudioSrv` to resolve audio desync, crackling, or high-latency buffer issues causing frame drops.
  - **RAM Standby Cache Flush:** Purges unmanaged standby cache (`EmptyWorkingSet`) to relieve immediate physical memory pressure.
  - **DNS Cache Cleanup:** Clears DNS resolver cache via `ipconfig /flushdns` to reduce network ping jitter.
- **🛠️ Full Service Restart:**
  - Includes all quick fix features plus restarts for **Windows Search Indexer (WSearch)**, **SysMain (Superfetch)**, **Windows Update & BITS**, and **Print Spooler** (resolves 100% Disk / 100% CPU lockups).
- **▶️ Selective Mode:** Check and run only the exact services you want to restart.
- **🛡️ Auto Administrator Elevation:** Embedded UAC manifest and self-elevation banner for restarting protected system services.
- **🎨 Modern Dark Theme GUI:** Clean, responsive, multi-threaded UI with real-time logging and emergency cancellation support.

---

## 🚀 Quick Start

### Option 1: Direct Run (No setup needed)
1. Download or clone this repository.
2. Run `Chay-FixLag.bat` or double-click `WinLagFixer.exe` (Accept the UAC Administrator prompt).
3. Click **"⚡ Khắc Phục Nhanh"** or choose individual services.

### Option 2: Desktop Shortcut
- Run `Tao-Shortcut-Desktop.bat` to automatically place a `⚡ Fix Lag Windows` shortcut on your Desktop.

---

## 🏗️ Building from Source

No heavy IDE or SDK required. It compiles natively using the built-in Microsoft .NET Framework C# compiler present on every Windows 10/11 system:

```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32manifest:app.manifest /out:WinLagFixer.exe /reference:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.ServiceProcess.dll,System.Core.dll Program.cs
```

---

## 📖 Vietnamese Documentation (Hướng Dẫn Tiếng Việt)

Chi tiết hướng dẫn sử dụng và giải thích hoạt động từng dịch vụ vui lòng xem tại file: [HUONG_DAN_SU_DUNG.md](HUONG_DAN_SU_DUNG.md).

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
