using System.IO;

namespace SplitWallpaper.App.Tests.Branding;

public sealed class AppMetadataTests
{
    [Fact]
    public void AppProject_ContainsProductAndVersionMetadata()
    {
        var projectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "SplitWallpaper.App.csproj"));

        var project = File.ReadAllText(projectPath);

        Assert.Contains("<Title>Split Wallpaper</Title>", project);
        Assert.Contains("<Product>Split Wallpaper</Product>", project);
        Assert.Contains("<Description>Split wallpaper composer for the primary Windows display.</Description>", project);
        Assert.Contains("<Version>1.0.3</Version>", project);
        Assert.Contains("<AssemblyVersion>1.0.3.0</AssemblyVersion>", project);
        Assert.Contains("<FileVersion>1.0.3.0</FileVersion>", project);
        Assert.Contains("<InformationalVersion>1.0.3</InformationalVersion>", project);
    }
}
