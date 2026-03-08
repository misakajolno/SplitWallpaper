using SplitWallpaper.Core.Models;

namespace SplitWallpaper.Core.Rendering;

public static class PreviewLayoutCalculator
{
    public static PreviewSurfaceSize CalculateMaxPreviewSize(double monitorWidth, double monitorHeight, double availableWidth, double availableHeight)
    {
        if (monitorWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monitorWidth));
        }

        if (monitorHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monitorHeight));
        }

        if (availableWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableWidth));
        }

        if (availableHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableHeight));
        }

        var aspectRatio = monitorWidth / monitorHeight;
        var width = availableWidth;
        var height = width / aspectRatio;

        if (height > availableHeight)
        {
            height = availableHeight;
            width = height * aspectRatio;
        }

        return new PreviewSurfaceSize(width, height);
    }
}
