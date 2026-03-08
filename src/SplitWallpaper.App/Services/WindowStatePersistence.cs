using SplitWallpaper.Core.Models;
using System.Windows;

namespace SplitWallpaper.App.Services;

public static class WindowStatePersistence
{
    public static WindowStateSnapshot Capture(
        double left,
        double top,
        double width,
        double height,
        Rect restoreBounds,
        WindowState windowState,
        double minWidth,
        double minHeight)
    {
        var bounds = windowState == WindowState.Maximized
            ? restoreBounds
            : new Rect(left, top, width, height);

        return new WindowStateSnapshot(
            bounds.Left,
            bounds.Top,
            Math.Max(minWidth, bounds.Width),
            Math.Max(minHeight, bounds.Height),
            windowState == WindowState.Maximized);
    }

    public static bool TryRestore(
        AppConfig? config,
        double minWidth,
        double minHeight,
        Rect visibleBounds,
        out WindowStateSnapshot? restoredState)
    {
        restoredState = null;

        if (config?.WindowWidth is null
            || config.WindowHeight is null
            || config.WindowLeft is null
            || config.WindowTop is null)
        {
            return false;
        }

        if (config.WindowWidth < minWidth || config.WindowHeight < minHeight)
        {
            return false;
        }

        var targetBounds = new Rect(config.WindowLeft.Value, config.WindowTop.Value, config.WindowWidth.Value, config.WindowHeight.Value);

        if (!targetBounds.IntersectsWith(visibleBounds))
        {
            return false;
        }

        restoredState = new WindowStateSnapshot(
            config.WindowLeft.Value,
            config.WindowTop.Value,
            config.WindowWidth.Value,
            config.WindowHeight.Value,
            config.IsWindowMaximized);

        return true;
    }
}

public sealed record WindowStateSnapshot(double Left, double Top, double Width, double Height, bool IsMaximized);
