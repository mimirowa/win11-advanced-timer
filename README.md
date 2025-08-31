# Win11 Advanced Timer

## Project Purpose
Win11 Advanced Timer is a Windows 11 timer application that exposes a full screen heads-up display (HUD) and a lightweight widget. The repository is scaffolding for experimentation with the Windows App SDK and widget framework.

## Accuracy Model
The HUD uses the high resolution system clock and updates every frame. The widget relies on the operating system's scheduled refresh cycle, so its display may lag behind the HUD by up to a few seconds. Both read from the same underlying service, but periodic refresh on the widget means minor drift is expected when the timer is active.

## Building a Release

### Prerequisites
- Windows 11
- Visual Studio 2022 with the Universal Windows Platform, .NET desktop, and Desktop development with C++ workloads
- MSIX packaging tools

### Steps
1. Clone the repository.
```powershell
git clone https://github.com/mimirowa/win11-advanced-timer.git
cd win11-advanced-timer
```
2. Restore dependencies.
```powershell
dotnet restore src/AdvancedTimer.sln
```
3. Publish the app.
```powershell
dotnet publish src/AdvancedTimer.App/AdvancedTimer.App.csproj -c Release -r win10-x64 --self-contained
```
4. Publish the widget provider.
```powershell
dotnet publish src/AdvancedTimer.WidgetProvider/AdvancedTimer.WidgetProvider.csproj -c Release -r win10-x64 --self-contained
```
5. Create the MSIX package.
```powershell
msbuild src/AdvancedTimer.App/AdvancedTimer.App.csproj `
    /p:Configuration=Release `
    /p:Platform=x64 `
    /p:GenerateAppxPackageOnBuild=true `
    /p:AppxBundle=Always
```

The packaged binaries appear under `src/AdvancedTimer.App/bin/Release`. Install the generated `.msixbundle` to test the application.

## Installation
The app is packaged as an MSIX for sideloading.
1. Enable developer mode or allow sideloading in Windows settings.
2. Download the `AdvancedTimer.msix` package from releases.
3. Install with PowerShell:
   ```powershell
   Add-AppxPackage .\AdvancedTimer.msix
   ```

## Controls
- **Start / Stop** – begin or cancel a timer.
- **Pause / Resume** – temporarily halt a running timer and continue later.
- **Reset** – clear the current timer values.

## Known Limitations
- Widget refresh intervals can cause visible drift compared with the HUD.
- App and widget are only tested on Windows 11; other platforms are unsupported.
- Sub-second accuracy is not guaranteed.

## Roadmap
- Persist timers across device restarts.
- Expose configurable refresh interval for widgets.
- Add sound or notification options when a timer completes.

## Manual Test Plan
1. **Timer start/finish** – start a timer and verify that it completes at the expected time.
2. **HUD vs. widget drift** – run a timer and compare the HUD count with the widget display to ensure drift stays within a few seconds.
3. **Pause/resume persistence** – pause a timer, close and reopen the app, then resume and confirm the remaining time is preserved.
4. **Multi-widget independence** – add multiple timer widgets and verify each maintains its own countdown without affecting others.
5. **Reboot persistence** – start a timer, reboot the machine, and confirm the timer continues from its pre-reboot state.

