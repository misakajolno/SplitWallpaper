namespace SplitWallpaper.Core.Models;

public readonly record struct SplitRegions(PixelRect Left, PixelRect Right, double EffectiveRatio);
