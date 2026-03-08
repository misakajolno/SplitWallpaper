using SplitWallpaper.Core.Models;

namespace SplitWallpaper.Core.Rendering;

public static class LayoutCalculator
{
    public const double MinimumSplitRatio = 0.1;
    public const double MaximumSplitRatio = 0.9;

    public static double ClampSplitRatio(double ratio)
    {
        if (ratio < MinimumSplitRatio)
        {
            return MinimumSplitRatio;
        }

        if (ratio > MaximumSplitRatio)
        {
            return MaximumSplitRatio;
        }

        return ratio;
    }

    public static int NormalizeSplitPercentage(double ratio)
    {
        var clampedRatio = ClampSplitRatio(ratio);
        return (int)Math.Round(clampedRatio * 100d, MidpointRounding.AwayFromZero);
    }

    public static double NormalizeSplitRatio(double ratio)
    {
        return NormalizeSplitPercentage(ratio) / 100d;
    }

    public static SplitRegions CalculateRegions(int totalWidth, int totalHeight, double splitRatio)
    {
        if (totalWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalWidth));
        }

        if (totalHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalHeight));
        }

        var requestedRatio = NormalizeSplitRatio(splitRatio);

        if (totalWidth == 1)
        {
            return new SplitRegions(
                new PixelRect(0, 0, 1, totalHeight),
                new PixelRect(1, 0, 0, totalHeight),
                1d);
        }

        var leftWidth = (int)Math.Round(totalWidth * requestedRatio, MidpointRounding.AwayFromZero);
        leftWidth = Math.Clamp(leftWidth, 1, totalWidth - 1);
        var rightWidth = totalWidth - leftWidth;
        var effectiveRatio = (double)leftWidth / totalWidth;

        return new SplitRegions(
            new PixelRect(0, 0, leftWidth, totalHeight),
            new PixelRect(leftWidth, 0, rightWidth, totalHeight),
            effectiveRatio);
    }

    public static DoubleRect CalculatePlacement(PixelSize source, PixelRect target, FillModeOption fillMode)
    {
        if (source.Width <= 0 || source.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(source));
        }

        if (target.Width <= 0 || target.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        return fillMode switch
        {
            FillModeOption.Stretch => new DoubleRect(target.X, target.Y, target.Width, target.Height),
            FillModeOption.Contain => ScaleToFit(source, target, Math.Min),
            FillModeOption.Cover => ScaleToFit(source, target, Math.Max),
            _ => throw new ArgumentOutOfRangeException(nameof(fillMode)),
        };
    }

    private static DoubleRect ScaleToFit(PixelSize source, PixelRect target, Func<double, double, double> scaleSelector)
    {
        var scaleX = (double)target.Width / source.Width;
        var scaleY = (double)target.Height / source.Height;
        var scale = scaleSelector(scaleX, scaleY);
        var width = source.Width * scale;
        var height = source.Height * scale;
        var x = target.X + ((target.Width - width) / 2d);
        var y = target.Y + ((target.Height - height) / 2d);

        return new DoubleRect(x, y, width, height);
    }
}
