using SplitWallpaper.Core.Rendering;

namespace SplitWallpaper.Tests.Rendering;

public sealed class PreviewLayoutCalculatorTests
{
    [Fact]
    public void CalculateMaxPreviewSize_FillsWidthWhenContainerIsWideEnough()
    {
        var result = PreviewLayoutCalculator.CalculateMaxPreviewSize(5120, 1440, 1200, 600);

        Assert.Equal(1200, result.Width, 3);
        Assert.Equal(337.5, result.Height, 3);
    }

    [Fact]
    public void CalculateMaxPreviewSize_FillsHeightWhenContainerHeightIsLimiting()
    {
        var result = PreviewLayoutCalculator.CalculateMaxPreviewSize(5120, 1440, 1200, 200);

        Assert.Equal(711.111, result.Width, 3);
        Assert.Equal(200, result.Height, 3);
    }

    [Fact]
    public void CalculateMaxPreviewSize_ReturnsPositiveValuesForNarrowContainers()
    {
        var result = PreviewLayoutCalculator.CalculateMaxPreviewSize(5120, 1440, 200, 500);

        Assert.True(result.Width > 0);
        Assert.True(result.Height > 0);
        Assert.Equal(200, result.Width, 3);
        Assert.Equal(56.25, result.Height, 3);
    }
}
