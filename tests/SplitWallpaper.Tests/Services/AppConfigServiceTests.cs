using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Services;

namespace SplitWallpaper.Tests.Services;

public sealed class AppConfigServiceTests
{
    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsConfig()
    {
        var service = new AppConfigService();
        var config = new AppConfig
        {
            LeftImagePath = @"C:\Images\left.png",
            RightImagePath = @"D:\Wallpapers\right.jpg",
            SplitRatio = 0.33,
            FillMode = FillModeOption.Contain,
            WindowWidth = 1500,
            WindowHeight = 980,
            WindowLeft = 120,
            WindowTop = 64,
            IsWindowMaximized = true,
        };

        var directory = Path.Combine(Path.GetTempPath(), $"SplitWallpaperTests-{Guid.NewGuid():N}");
        var configPath = Path.Combine(directory, "config.json");

        try
        {
            await service.SaveAsync(configPath, config);
            var loaded = await service.LoadAsync(configPath);

            Assert.NotNull(loaded);
            Assert.Equal(config.LeftImagePath, loaded!.LeftImagePath);
            Assert.Equal(config.RightImagePath, loaded.RightImagePath);
            Assert.Equal(config.SplitRatio, loaded.SplitRatio, 3);
            Assert.Equal(config.FillMode, loaded.FillMode);
            Assert.Equal(config.WindowWidth, loaded.WindowWidth);
            Assert.Equal(config.WindowHeight, loaded.WindowHeight);
            Assert.Equal(config.WindowLeft, loaded.WindowLeft);
            Assert.Equal(config.WindowTop, loaded.WindowTop);
            Assert.Equal(config.IsWindowMaximized, loaded.IsWindowMaximized);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task LoadAsync_ReturnsNullWhenFileDoesNotExist()
    {
        var service = new AppConfigService();
        var configPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        var loaded = await service.LoadAsync(configPath);

        Assert.Null(loaded);
    }
}
