using SplitWallpaper.Core.Models;
using System.Runtime.InteropServices;

namespace SplitWallpaper.App.Services;

public sealed class ScreenInfoService : IScreenInfoService
{
    public PixelSize GetPrimaryScreenSize() => new(GetSystemMetrics(PrimaryScreenWidthIndex), GetSystemMetrics(PrimaryScreenHeightIndex));

    private const int PrimaryScreenWidthIndex = 0;
    private const int PrimaryScreenHeightIndex = 1;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int systemMetricIndex);
}
