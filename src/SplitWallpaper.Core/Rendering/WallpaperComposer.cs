using SplitWallpaper.Core.Models;

namespace SplitWallpaper.Core.Rendering;

public sealed class WallpaperComposer : IWallpaperComposer
{
    public BgraBitmap Compose(BgraBitmap left, BgraBitmap right, PixelSize targetSize, double splitRatio, FillModeOption fillMode)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (targetSize.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetSize));
        }

        if (targetSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetSize));
        }

        var output = new BgraBitmap(targetSize.Width, targetSize.Height, new byte[targetSize.Width * targetSize.Height * 4]);
        var regions = LayoutCalculator.CalculateRegions(targetSize.Width, targetSize.Height, splitRatio);

        RenderInto(output, left, regions.Left, fillMode);
        RenderInto(output, right, regions.Right, fillMode);

        return output;
    }

    private static void RenderInto(BgraBitmap destination, BgraBitmap source, PixelRect targetRegion, FillModeOption fillMode)
    {
        var placement = LayoutCalculator.CalculatePlacement(new PixelSize(source.Width, source.Height), targetRegion, fillMode);

        for (var y = targetRegion.Y; y < targetRegion.Y + targetRegion.Height; y++)
        {
            if (!TryMapCoordinate(y + 0.5, placement.Y, placement.Height, source.Height, out var sourceY))
            {
                continue;
            }

            for (var x = targetRegion.X; x < targetRegion.X + targetRegion.Width; x++)
            {
                if (!TryMapCoordinate(x + 0.5, placement.X, placement.Width, source.Width, out var sourceX))
                {
                    continue;
                }

                var sourceIndex = ((sourceY * source.Width) + sourceX) * 4;
                var destinationIndex = ((y * destination.Width) + x) * 4;

                destination.Pixels[destinationIndex] = source.Pixels[sourceIndex];
                destination.Pixels[destinationIndex + 1] = source.Pixels[sourceIndex + 1];
                destination.Pixels[destinationIndex + 2] = source.Pixels[sourceIndex + 2];
                destination.Pixels[destinationIndex + 3] = source.Pixels[sourceIndex + 3];
            }
        }
    }

    private static bool TryMapCoordinate(double pixelCenter, double origin, double length, int sourceLength, out int sourceIndex)
    {
        sourceIndex = 0;

        if (length <= 0)
        {
            return false;
        }

        var normalized = (pixelCenter - origin) / length;

        if (normalized < 0 || normalized >= 1)
        {
            return false;
        }

        sourceIndex = Math.Clamp((int)(normalized * sourceLength), 0, sourceLength - 1);
        return true;
    }
}
