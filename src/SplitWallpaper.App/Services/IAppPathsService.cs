namespace SplitWallpaper.App.Services;

public interface IAppPathsService
{
    string RootDirectory { get; }

    string ConfigPath { get; }

    string GeneratedWallpaperPath { get; }
}
