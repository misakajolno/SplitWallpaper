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

    [Theory]
    [InlineData(0.503, 50)]
    [InlineData(0.505, 51)]
    [InlineData(0.507, 51)]
    [InlineData(0.104, 10)]
    [InlineData(0.106, 11)]
    [InlineData(0.0, 10)]
    [InlineData(1.0, 90)]
    public void NormalizeSplitPercentage_SnapsToWholePercent(double input, int expected)
    {
        var actual = LayoutCalculator.NormalizeSplitPercentage(input);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0.503, 0.50)]
    [InlineData(0.505, 0.51)]
    [InlineData(0.507, 0.51)]
    public void NormalizeSplitRatio_UsesWholePercentSteps(double input, double expected)
    {
        var actual = LayoutCalculator.NormalizeSplitRatio(input);

        Assert.Equal(expected, actual, 10);
    }
}
