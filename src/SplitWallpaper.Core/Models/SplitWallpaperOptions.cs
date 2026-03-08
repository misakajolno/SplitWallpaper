namespace SplitWallpaper.Core.Models;

public sealed record SplitWallpaperOptions(string? LeftImagePath, string? RightImagePath, double SplitRatio, FillModeOption FillMode)
{
    public SplitWallpaperOptions WithClampedSplitRatio(double splitRatio) => this with { SplitRatio = splitRatio < 0.1 ? 0.1 : splitRatio > 0.9 ? 0.9 : splitRatio };
}
