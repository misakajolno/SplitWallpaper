using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;

namespace SplitWallpaper.Tests.Rendering;

public sealed class WallpaperComposerTests
{
    [Fact]
    public void Compose_ReturnsBitmapWithRequestedSize()
    {
        var composer = new WallpaperComposer();
        var left = CreateSolidBitmap(16, 16, 0, 0, 255);
        var right = CreateSolidBitmap(16, 16, 255, 0, 0);

        var result = composer.Compose(left, right, new PixelSize(400, 200), 0.25, FillModeOption.Stretch);

        Assert.Equal(400, result.Width);
        Assert.Equal(200, result.Height);
    }

    [Fact]
    public void Compose_RendersLeftAndRightHalvesIntoCorrectRegions()
    {
        var composer = new WallpaperComposer();
        var left = CreateSolidBitmap(10, 10, 0, 0, 255);
        var right = CreateSolidBitmap(10, 10, 255, 0, 0);

        var result = composer.Compose(left, right, new PixelSize(400, 200), 0.25, FillModeOption.Stretch);

        Assert.Equal((0, 0, 255, 255), ReadPixel(result, 50, 100));
        Assert.Equal((255, 0, 0, 255), ReadPixel(result, 350, 100));
    }

    private static BgraBitmap CreateSolidBitmap(int width, int height, byte blue, byte green, byte red, byte alpha = 255)
    {
        var pixels = new byte[width * height * 4];

        for (var index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = blue;
            pixels[index + 1] = green;
            pixels[index + 2] = red;
            pixels[index + 3] = alpha;
        }

        return new BgraBitmap(width, height, pixels);
    }

    private static (byte Blue, byte Green, byte Red, byte Alpha) ReadPixel(BgraBitmap bitmap, int x, int y)
    {
        var index = ((y * bitmap.Width) + x) * 4;

        return (bitmap.Pixels[index], bitmap.Pixels[index + 1], bitmap.Pixels[index + 2], bitmap.Pixels[index + 3]);
    }
}
