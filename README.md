# OffTimer

OffTimer is a small portable shutdown timer for Windows 10 and Windows 11.

It is written in C# WinForms and is intended to be published as a self-contained single-file `.exe`.

Russian readme: [README.ru.md](README.ru.md)

## Features

- Portable app, no installer required.
- Custom shutdown timer in minutes.
- Quick timer buttons: `30`, `45`, `60`, `90`.
- Cancel active timer.
- Tray icon support.
- Closing the main window hides the app to tray instead of stopping the timer.
- Only one app instance is allowed. Running OffTimer again opens the existing window.
- Click-through always-on-top overlay.
- Overlay settings:
  - enabled / disabled;
  - font size;
  - color;
  - screen corner.
- The overlay shows only digits:
  - more than 60 seconds left: remaining minutes, for example `43`, `13`, `09`;
  - last minute: remaining seconds, for example `60`, `59`, `58`.
- If the overlay is disabled, it still appears automatically during the last 60 seconds.
- Russian and English interface.
- Language modes: auto, russian, english.

## System requirements

- Windows 10 or Windows 11.
- No .NET runtime is required for published self-contained builds.
- .NET 10 SDK is required only for building from source.

## How shutdown works

OffTimer uses the standard Windows shutdown tool:

```powershell
shutdown.exe /s /t <seconds>
```

Timer cancellation uses:

```powershell
shutdown.exe /a
```

The app performs a normal shutdown. It does not force-close apps with `/f`.

## Portable settings

OffTimer stores settings in:

```text
offtimer/settings.json
```

The app first tries to create this folder next to `OffTimer.exe`.

If the executable folder is not writable, OffTimer falls back to:

```text
%AppData%\OffTimer\settings.json
```

The executable is not moved automatically.

## Build from source

Install the .NET 10 SDK, then run:

```powershell
dotnet restore src/OffTimer/OffTimer.csproj
```

Publish Windows x64:

```powershell
dotnet publish src/OffTimer/OffTimer.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:DebugType=embedded
```

Publish Windows x86:

```powershell
dotnet publish src/OffTimer/OffTimer.csproj -c Release -r win-x86 --self-contained true -o publish/win-x86 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:DebugType=embedded
```

The output folder will contain `OffTimer.exe`.

## GitHub Actions

The repository includes a workflow:

```text
.github/workflows/build.yml
```

It builds both:

- `win-x64`
- `win-x86`

The workflow can be started manually from the GitHub Actions tab.

## Project structure

```text
OffTimer.sln
src/OffTimer/
  AppPaths.cs
  AppSettings.cs
  CountdownController.cs
  Localizer.cs
  MainForm.cs
  NativeMethods.cs
  OverlayForm.cs
  Program.cs
  SettingsForm.cs
  SettingsService.cs
  ShutdownService.cs
  SingleInstance.cs
  assets/offtimer.ico
.github/workflows/build.yml
README.md
README.ru.md
LICENSE
```

## Notes

This is an early version. Before using it as a daily utility, test:

- timer start;
- timer cancellation;
- tray behavior;
- overlay behavior;
- x64 and x86 builds;
- shutdown cancellation from the app and from `shutdown /a`.

## License

MIT
