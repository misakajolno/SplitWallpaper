using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;

namespace SplitWallpaper.Tests.Rendering;

public sealed class SplitRatioTests
{
    [Theory]
    [InlineData(0.0, 0.1)]
    [InlineData(0.05, 0.1)]
    [InlineData(0.5, 0.5)]
    [InlineData(0.95, 0.9)]
    [InlineData(1.0, 0.9)]
    public void ClampSplitRatio_UsesSafeBounds(double input, double expected)
    {
        var actual = LayoutCalculator.ClampSplitRatio(input);

        Assert.Equal(expected, actual, 3);
    }
}
