using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;

namespace SplitWallpaper.Tests.Rendering;

public sealed class LayoutCalculatorEdgeCaseTests
{
    [Fact]
    public void CalculateRegions_WithSinglePixelWidth_DoesNotThrow()
    {
        var result = LayoutCalculator.CalculateRegions(1, 10, 0.5);

        Assert.Equal(new PixelRect(0, 0, 1, 10), result.Left);
        Assert.Equal(new PixelRect(1, 0, 0, 10), result.Right);
    }
}
