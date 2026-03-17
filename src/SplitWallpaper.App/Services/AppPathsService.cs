using System.IO;

namespace SplitWallpaper.App.Services;

public sealed class AppPathsService : IAppPathsService
{
    public AppPathsService()
    {
        RootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SplitWallpaper");
        ConfigPath = Path.Combine(RootDirectory, "config.json");
        GeneratedWallpaperPath = Path.Combine(RootDirectory, "generated-wallpaper.bmp");
    }

    public string RootDirectory { get; }

    public string ConfigPath { get; }

    public string GeneratedWallpaperPath { get; }
}
