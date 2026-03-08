# Split Wallpaper

[中文说明 / Chinese README](README.md)

Split Wallpaper is a Windows-only desktop app that splits the primary display into left and right regions and applies different wallpapers to each side.

## Features

- Split wallpaper composition for the primary display
- Live preview that matches the primary monitor aspect ratio
- Multiple fill modes for different image sizes
- Adjustable split ratio
- Built with `.NET 8` and `WPF`

## Requirements

- Windows 10 or Windows 11
- Git
- `.NET 8 SDK`
- Optional: `VS Code` or any C#-capable editor

## Install .NET 8 SDK

Recommended:

```powershell
winget install Microsoft.DotNet.SDK.8
dotnet --version
```

If `dotnet --version` prints an `8.x` version, the SDK is ready.

Official resources:

- `.NET 8 download: https://dotnet.microsoft.com/download/dotnet/8.0`
- `Windows install guide: https://learn.microsoft.com/dotnet/core/install/windows`

Notes:

- The `.NET 8 SDK` already includes what you need for local development
- If you only want to run the published `framework-dependent` package, install `.NET Desktop Runtime 8` on the target machine

## Quick Start

Clone the repository:

```powershell
git clone <your-repo-url>
cd SplitWallpaper
```

Restore dependencies:

```powershell
dotnet restore
```

Build:

```powershell
dotnet build
```

Run tests:

```powershell
dotnet test
```

Run the app:

```powershell
dotnet run --project .\src\SplitWallpaper.App\SplitWallpaper.App.csproj
```

## Publish

Create a framework-dependent release package:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-framework-dependent.ps1
```

Output folder:

```text
release\framework-dependent
```

If the target machine doesn't have the SDK, install the desktop runtime:

```powershell
winget install Microsoft.DotNet.DesktopRuntime.8
```

## Project Structure

```text
SplitWallpaper
├─ src/                         application and core library source
│  ├─ SplitWallpaper.App/
│  └─ SplitWallpaper.Core/
├─ tests/                       unit and app-level tests
├─ scripts/                     publish scripts
├─ README.md                    Chinese homepage
├─ README.en.md                 English documentation
└─ SplitWallpaper.sln           solution file
```

## FAQ

### Do I need Visual Studio?

No. With `.NET 8 SDK` installed, you can build, test, run, and publish from PowerShell, Command Prompt, or `VS Code`.

### Why is this project Windows-only?

The app is built with `WPF` and targets the Windows wallpaper workflow, so it doesn't support macOS or Linux.

### Why are `.dotnet`, `.tooling`, and `release` not in the repository?

They are local SDK/tool caches or generated outputs, so they are intentionally excluded from the GitHub source repository.

### Why does the publish script still work without a local `.dotnet` folder?

The script first tries `.\.dotnet\dotnet.exe` and falls back to the system `dotnet` command if the local SDK isn't present.

## License

This project is released under the `MIT` license. See `LICENSE` in the repository root for details.
