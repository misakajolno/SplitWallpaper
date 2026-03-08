using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;

namespace SplitWallpaper.Tests.Rendering;

public sealed class LayoutCalculatorTests
{
    [Fact]
    public void CalculateRegions_UsesEffectiveSplitRatio()
    {
        var result = LayoutCalculator.CalculateRegions(1920, 1080, 0.25);

        Assert.Equal(new PixelRect(0, 0, 480, 1080), result.Left);
        Assert.Equal(new PixelRect(480, 0, 1440, 1080), result.Right);
        Assert.Equal(0.25, result.EffectiveRatio, 3);
    }

    [Fact]
    public void CalculateRegions_SnapsFractionalPercentBeforeCalculatingRegions()
    {
        var result = LayoutCalculator.CalculateRegions(5120, 1440, 0.503);

        Assert.Equal(new PixelRect(0, 0, 2560, 1440), result.Left);
        Assert.Equal(new PixelRect(2560, 0, 2560, 1440), result.Right);
        Assert.Equal(0.5, result.EffectiveRatio, 10);
    }

    [Fact]
    public void CalculatePlacement_DifferentiatesFillModes()
    {
        var source = new PixelSize(1000, 500);
        var target = new PixelRect(0, 0, 400, 300);

        var cover = LayoutCalculator.CalculatePlacement(source, target, FillModeOption.Cover);
        var contain = LayoutCalculator.CalculatePlacement(source, target, FillModeOption.Contain);
        var stretch = LayoutCalculator.CalculatePlacement(source, target, FillModeOption.Stretch);

        Assert.Equal(new DoubleRect(-100, 0, 600, 300), cover);
        Assert.Equal(new DoubleRect(0, 50, 400, 200), contain);
        Assert.Equal(new DoubleRect(0, 0, 400, 300), stretch);
    }
}
