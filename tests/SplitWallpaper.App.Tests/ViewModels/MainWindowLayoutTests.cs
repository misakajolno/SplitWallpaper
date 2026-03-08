using System.IO;

namespace SplitWallpaper.App.Tests.ViewModels;

public sealed class MainWindowLayoutTests
{
    [Fact]
    public void MainWindow_ContainsPrimaryDisplaySummaryAndStackedCards()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "MainWindow.xaml"));

        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("x:Name=\"SettingsCard\"", xaml);
        Assert.Contains("x:Name=\"PreviewCard\"", xaml);
        Assert.Contains("x:Name=\"StatusCard\"", xaml);
        Assert.Contains("Text=\"{Binding PrimaryDisplaySummaryText}\"", xaml);
        Assert.Contains("Grid.Row=\"2\"", xaml);
    }

    [Fact]
    public void MainWindow_UsesSingleDisplaySummaryInPreviewHeader()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "MainWindow.xaml"));

        var xaml = File.ReadAllText(xamlPath);
        var displaySummaryBinding = "Text=\"{Binding PrimaryDisplaySummaryText}\"";

        Assert.Equal(1, xaml.Split(displaySummaryBinding).Length - 1);
        Assert.Contains("DockPanel.Dock=\"Right\"", xaml);
    }

    [Fact]
    public void MainWindow_UsesPreviewSurfaceBindingsAndOneWayPathBindings()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "MainWindow.xaml"));

        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("Width=\"{Binding PreviewSurfaceWidth}\"", xaml);
        Assert.Contains("Height=\"{Binding PreviewSurfaceHeight}\"", xaml);
        Assert.Contains("Text=\"{Binding LeftImagePath, Mode=OneWay, TargetNullValue=尚未选择}\"", xaml);
        Assert.Contains("Text=\"{Binding RightImagePath, Mode=OneWay, TargetNullValue=尚未选择}\"", xaml);
        Assert.Contains("Style=\"{StaticResource WindowSliderStyle}\"", xaml);
    }

    [Fact]
    public void MainWindow_UsesTallerWindowAndLocalizedFillModeBinding()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "MainWindow.xaml"));

        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("Height=\"1080\"", xaml);
        Assert.Contains("DisplayMemberPath=\"DisplayName\"", xaml);
        Assert.Contains("SelectedValuePath=\"Value\"", xaml);
        Assert.Contains("SelectedValue=\"{Binding SelectedFillMode, Mode=TwoWay}\"", xaml);
    }
}





