using SplitWallpaper.App.Services;
using SplitWallpaper.Core.Models;
using System.Windows;

namespace SplitWallpaper.App.Tests.Services;

public sealed class WindowStatePersistenceTests
{
    [Fact]
    public void Capture_UsesCurrentBoundsForNormalWindow()
    {
        var result = WindowStatePersistence.Capture(
            left: 120,
            top: 64,
            width: 1440,
            height: 960,
            restoreBounds: new Rect(20, 30, 900, 700),
            windowState: WindowState.Normal,
            minWidth: 1100,
            minHeight: 760);

        Assert.Equal(120, result.Left);
        Assert.Equal(64, result.Top);
        Assert.Equal(1440, result.Width);
        Assert.Equal(960, result.Height);
        Assert.False(result.IsMaximized);
    }

    [Fact]
    public void Capture_UsesRestoreBoundsForMaximizedWindow()
    {
        var result = WindowStatePersistence.Capture(
            left: 0,
            top: 0,
            width: 1920,
            height: 1080,
            restoreBounds: new Rect(40, 50, 1500, 980),
            windowState: WindowState.Maximized,
            minWidth: 1100,
            minHeight: 760);

        Assert.Equal(40, result.Left);
        Assert.Equal(50, result.Top);
        Assert.Equal(1500, result.Width);
        Assert.Equal(980, result.Height);
        Assert.True(result.IsMaximized);
    }

    [Fact]
    public void TryRestore_ReturnsSnapshotForVisiblePersistedState()
    {
        var config = new AppConfig
        {
            WindowWidth = 1500,
            WindowHeight = 980,
            WindowLeft = 120,
            WindowTop = 64,
            IsWindowMaximized = true,
        };

        var success = WindowStatePersistence.TryRestore(
            config,
            minWidth: 1100,
            minHeight: 760,
            visibleBounds: new Rect(0, 0, 3840, 2160),
            out var restoredState);

        Assert.True(success);
        Assert.NotNull(restoredState);
        Assert.Equal(1500, restoredState!.Width);
        Assert.Equal(980, restoredState.Height);
        Assert.Equal(120, restoredState.Left);
        Assert.Equal(64, restoredState.Top);
        Assert.True(restoredState.IsMaximized);
    }

    [Fact]
    public void TryRestore_RejectsOffScreenPlacement()
    {
        var config = new AppConfig
        {
            WindowWidth = 1500,
            WindowHeight = 980,
            WindowLeft = 6000,
            WindowTop = 4000,
            IsWindowMaximized = false,
        };

        var success = WindowStatePersistence.TryRestore(
            config,
            minWidth: 1100,
            minHeight: 760,
            visibleBounds: new Rect(0, 0, 3840, 2160),
            out var restoredState);

        Assert.False(success);
        Assert.Null(restoredState);
    }
}
