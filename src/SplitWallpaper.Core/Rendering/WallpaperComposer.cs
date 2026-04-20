using SplitWallpaper.Core.Models;

namespace SplitWallpaper.Core.Rendering;

public sealed class WallpaperComposer : IWallpaperComposer
{
    public BgraBitmap Compose(
        BgraBitmap left,
        BgraBitmap right,
        PixelSize targetSize,
        double splitRatio,
        FillModeOption fillMode,
        ImageOffset leftOffset = default,
        ImageOffset rightOffset = default)
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

        RenderInto(output, left, regions.Left, fillMode, leftOffset);
        RenderInto(output, right, regions.Right, fillMode, rightOffset);

        return output;
    }

    private static void RenderInto(BgraBitmap destination, BgraBitmap source, PixelRect targetRegion, FillModeOption fillMode, ImageOffset offset)
    {
        var placement = LayoutCalculator.CalculatePlacement(new PixelSize(source.Width, source.Height), targetRegion, fillMode, offset);

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

                var destinationIndex = ((y * destination.Width) + x) * 4;
                SamplePixelBilinear(source, sourceX, sourceY, destination.Pixels, destinationIndex);
            }
        }
    }

    private static bool TryMapCoordinate(double pixelCenter, double origin, double length, int sourceLength, out double sourceCoordinate)
    {
        sourceCoordinate = 0;

        if (length <= 0)
        {
            return false;
        }

        var normalized = (pixelCenter - origin) / length;

        if (normalized < 0 || normalized >= 1)
        {
            return false;
        }

        sourceCoordinate = (normalized * sourceLength) - 0.5;
        return true;
    }

    private static void SamplePixelBilinear(BgraBitmap source, double sourceX, double sourceY, byte[] destinationPixels, int destinationIndex)
    {
        var x0 = (int)Math.Floor(sourceX);
        var y0 = (int)Math.Floor(sourceY);
        var x1 = x0 + 1;
        var y1 = y0 + 1;

        var xWeight = sourceX - x0;
        var yWeight = sourceY - y0;

        x0 = Math.Clamp(x0, 0, source.Width - 1);
        y0 = Math.Clamp(y0, 0, source.Height - 1);
        x1 = Math.Clamp(x1, 0, source.Width - 1);
        y1 = Math.Clamp(y1, 0, source.Height - 1);

        for (var channel = 0; channel < 4; channel++)
        {
            var topLeft = ReadChannel(source, x0, y0, channel);
            var topRight = ReadChannel(source, x1, y0, channel);
            var bottomLeft = ReadChannel(source, x0, y1, channel);
            var bottomRight = ReadChannel(source, x1, y1, channel);

            var top = Lerp(topLeft, topRight, xWeight);
            var bottom = Lerp(bottomLeft, bottomRight, xWeight);
            var value = Lerp(top, bottom, yWeight);

            destinationPixels[destinationIndex + channel] = (byte)Math.Clamp((int)Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    private static byte ReadChannel(BgraBitmap bitmap, int x, int y, int channel)
    {
        var index = ((y * bitmap.Width) + x) * 4;
        return bitmap.Pixels[index + channel];
    }

    private static double Lerp(double start, double end, double amount)
    {
        return start + ((end - start) * amount);
    }
}
