using System.IO;

namespace SplitWallpaper.App.Tests.Branding;

public sealed class AppIconTests
{
    [Fact]
    public void AppProject_ReferencesApplicationIconAsset()
    {
        var projectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "SplitWallpaper.App.csproj"));

        var project = File.ReadAllText(projectPath);

        Assert.Contains("<ApplicationIcon>Assets\\AppIcon.ico</ApplicationIcon>", project);
    }

    [Fact]
    public void AppIconAssets_Exist()
    {
        var iconPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "Assets", "AppIcon.ico"));

        var pngPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "Assets", "AppIcon.png"));

        Assert.True(File.Exists(iconPath), $"Icon file not found: {iconPath}");
        Assert.True(File.Exists(pngPath), $"PNG icon file not found: {pngPath}");
    }
}
