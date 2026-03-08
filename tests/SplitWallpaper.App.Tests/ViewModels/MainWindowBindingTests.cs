using System.IO;

namespace SplitWallpaper.App.Tests.ViewModels;

public sealed class MainWindowBindingTests
{
    [Fact]
    public void MainWindow_ReadOnlyPathBindings_AreExplicitlyOneWay()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "SplitWallpaper.App", "MainWindow.xaml"));

        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("Text=\"{Binding LeftImagePath, Mode=OneWay, TargetNullValue=尚未选择}\"", xaml);
        Assert.Contains("Text=\"{Binding RightImagePath, Mode=OneWay, TargetNullValue=尚未选择}\"", xaml);
    }
}
