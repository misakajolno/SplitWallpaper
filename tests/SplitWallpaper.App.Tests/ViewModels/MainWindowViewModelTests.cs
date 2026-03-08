using SplitWallpaper.App.Services;
using SplitWallpaper.App.ViewModels;
using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;
using SplitWallpaper.Core.Services;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SplitWallpaper.App.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public async Task InitializeAsync_ExposesPrimaryDisplayMetadata()
    {
        var viewModel = CreateViewModel(screenInfoService: new FakeScreenInfoService(new PixelSize(5120, 1440)));

        await viewModel.InitializeAsync();

        Assert.Equal("5120 × 1440", viewModel.PrimaryDisplayResolutionText);
        Assert.Equal("32:9", viewModel.PrimaryDisplayAspectText);
        Assert.Contains("主显示器", viewModel.PrimaryDisplaySummaryText);
    }

    [Fact]
    public void UpdatePreviewSize_UsesPrimaryDisplayAspectRatio()
    {
        var viewModel = CreateViewModel(screenInfoService: new FakeScreenInfoService(new PixelSize(5120, 1440)));

        viewModel.UpdatePreviewSize(1000, 600);

        Assert.Equal(1000, viewModel.PreviewSurfaceWidth, 3);
        Assert.Equal(281.25, viewModel.PreviewSurfaceHeight, 3);
        Assert.Equal(5120d / 1440d, viewModel.PreviewSurfaceWidth / viewModel.PreviewSurfaceHeight, 3);
    }

    [Fact]
    public void UpdatePreviewSize_RemainsAspectCorrectWhenHeightLimited()
    {
        var viewModel = CreateViewModel(screenInfoService: new FakeScreenInfoService(new PixelSize(5120, 1440)));

        viewModel.UpdatePreviewSize(1000, 120);

        Assert.Equal(426.667, viewModel.PreviewSurfaceWidth, 3);
        Assert.Equal(120, viewModel.PreviewSurfaceHeight, 3);
        Assert.Equal(5120d / 1440d, viewModel.PreviewSurfaceWidth / viewModel.PreviewSurfaceHeight, 3);
    }

    [Fact]
    public void SetSplitRatioFromPreview_ClampsLowValues()
    {
        var viewModel = CreateViewModel();

        viewModel.SetSplitRatioFromPreview(0.0);

        Assert.Equal(0.1, viewModel.SplitRatio, 3);
    }

    [Fact]
    public void FillModes_ExposeChineseDisplayNames()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(
            new[] { "裁剪填充", "完整显示", "拉伸铺满" },
            viewModel.FillModes.Select(mode => mode.DisplayName).ToArray());
    }

    [Fact]
    public async Task ApplyAsync_UsesPrimaryDisplaySizeInsteadOfPreviewSize()
    {
        var configService = new FakeAppConfigService();
        var pathService = new FakeAppPathsService();
        var conversionService = new FakeBitmapConversionService();
        var wallpaperService = new FakeWallpaperService();
        var screenInfoService = new FakeScreenInfoService(new PixelSize(1920, 1080));
        var wallpaperComposer = new FakeWallpaperComposer();
        var viewModel = CreateViewModel(configService, pathService, conversionService, screenInfoService, wallpaperService, wallpaperComposer);

        conversionService.Register("left", CreateBitmap(32, 32, 0, 0, 255));
        conversionService.Register("right", CreateBitmap(32, 32, 255, 0, 0));
        viewModel.UpdatePreviewSize(500, 200);
        viewModel.SetLeftImagePath("left");
        viewModel.SetRightImagePath("right");

        await viewModel.ApplyAsync();

        Assert.True(wallpaperComposer.RequestedSizes.Count >= 2);
        Assert.Contains(wallpaperComposer.RequestedSizes, size => size.Width < 1920 && size.Height < 1080);
        Assert.Equal(new PixelSize(1920, 1080), wallpaperComposer.RequestedSizes[^1]);
        Assert.Equal(pathService.GeneratedWallpaperPath, wallpaperService.AppliedPath);
        Assert.NotNull(configService.LastSavedConfig);
    }

    private static MainWindowViewModel CreateViewModel(
        IAppConfigService? configService = null,
        IAppPathsService? pathService = null,
        IBgraBitmapConversionService? conversionService = null,
        IScreenInfoService? screenInfoService = null,
        IWallpaperService? wallpaperService = null,
        IWallpaperComposer? wallpaperComposer = null)
    {
        return new MainWindowViewModel(
            configService ?? new FakeAppConfigService(),
            pathService ?? new FakeAppPathsService(),
            conversionService ?? new FakeBitmapConversionService(),
            wallpaperComposer ?? new WallpaperComposer(),
            screenInfoService ?? new FakeScreenInfoService(new PixelSize(2560, 1440)),
            wallpaperService ?? new FakeWallpaperService());
    }

    private static BgraBitmap CreateBitmap(int width, int height, byte blue, byte green, byte red)
    {
        var pixels = new byte[width * height * 4];

        for (var index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = blue;
            pixels[index + 1] = green;
            pixels[index + 2] = red;
            pixels[index + 3] = 255;
        }

        return new BgraBitmap(width, height, pixels);
    }

    private sealed class FakeAppConfigService : IAppConfigService
    {
        public AppConfig? LastSavedConfig { get; private set; }

        public Task<AppConfig?> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AppConfig?>(null);
        }

        public Task SaveAsync(string path, AppConfig config, CancellationToken cancellationToken = default)
        {
            LastSavedConfig = config;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAppPathsService : IAppPathsService
    {
        public string RootDirectory => "C:/fake";

        public string ConfigPath => "C:/fake/config.json";

        public string GeneratedWallpaperPath => "C:/fake/generated.png";
    }

    private sealed class FakeBitmapConversionService : IBgraBitmapConversionService
    {
        private readonly Dictionary<string, BgraBitmap> _registered = new();

        public void Register(string key, BgraBitmap bitmap)
        {
            _registered[key] = bitmap;
        }

        public BgraBitmap LoadFromFile(string path)
        {
            return _registered[path];
        }

        public void SaveAsPng(BgraBitmap bitmap, string path)
        {
        }

        public BitmapSource ToBitmapSource(BgraBitmap bitmap)
        {
            return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 255 }, 4);
        }
    }

    private sealed class FakeScreenInfoService : IScreenInfoService
    {
        private readonly PixelSize _screenSize;

        public FakeScreenInfoService(PixelSize screenSize)
        {
            _screenSize = screenSize;
        }

        public PixelSize GetPrimaryScreenSize() => _screenSize;
    }

    private sealed class FakeWallpaperService : IWallpaperService
    {
        public string? AppliedPath { get; private set; }

        public void ApplyWallpaper(string path)
        {
            AppliedPath = path;
        }
    }

    private sealed class FakeWallpaperComposer : IWallpaperComposer
    {
        public List<PixelSize> RequestedSizes { get; } = new();

        public BgraBitmap Compose(BgraBitmap left, BgraBitmap right, PixelSize targetSize, double splitRatio, FillModeOption fillMode)
        {
            RequestedSizes.Add(targetSize);
            return CreateBitmap(targetSize.Width, targetSize.Height, 0, 0, 0);
        }
    }
}
