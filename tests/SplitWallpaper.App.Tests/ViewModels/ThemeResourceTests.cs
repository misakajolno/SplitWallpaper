using System.IO;

namespace SplitWallpaper.App.Tests.ViewModels;

public sealed class ThemeResourceTests
{
    [Fact]
    public void ThemeResourceDictionary_ExistsWithCoreStyleKeys()
    {
        var themePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "Styles", "Theme.xaml"));

        Assert.True(File.Exists(themePath), $"Theme file not found: {themePath}");

        var xaml = File.ReadAllText(themePath);
        Assert.Contains("x:Key=\"CardBorderStyle\"", xaml);
        Assert.Contains("x:Key=\"PrimaryButtonStyle\"", xaml);
        Assert.Contains("x:Key=\"SecondaryButtonStyle\"", xaml);
        Assert.Contains("x:Key=\"WindowComboBoxItemStyle\"", xaml);
        Assert.Contains("x:Key=\"WindowComboBoxToggleButtonStyle\"", xaml);
        Assert.Contains("SelectedItem.DisplayName", xaml);
        Assert.Contains("x:Key=\"WindowSliderStyle\"", xaml);
    }

    [Fact]
    public void AppXaml_MergesThemeResourceDictionary()
    {
        var appPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "App.xaml"));

        var xaml = File.ReadAllText(appPath);
        Assert.Contains("Styles/Theme.xaml", xaml);
    }
}
