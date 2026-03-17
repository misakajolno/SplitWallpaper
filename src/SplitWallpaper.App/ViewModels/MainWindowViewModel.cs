using SplitWallpaper.App.Services;
using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;
using SplitWallpaper.Core.Services;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace SplitWallpaper.App.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IAppConfigService _appConfigService;
    private readonly IAppPathsService _appPathsService;
    private readonly IBgraBitmapConversionService _bitmapConversionService;
    private readonly IWallpaperComposer _wallpaperComposer;
    private readonly IScreenInfoService _screenInfoService;
    private readonly IWallpaperService _wallpaperService;

    private readonly PixelSize _primaryDisplaySize;
    private string? _leftImagePath;
    private string? _rightImagePath;
    private BgraBitmap? _leftBitmap;
    private BgraBitmap? _rightBitmap;
    private int _splitPercentage = 50;
    private FillModeOption _selectedFillMode = FillModeOption.Cover;
    private BitmapSource? _previewImageSource;
    private string _statusMessage = "请选择左右图片开始预览。";
    private bool _isBusy;
    private double _previewHostWidth = 960;
    private double _previewHostHeight = 540;
    private double _previewSurfaceWidth;
    private double _previewSurfaceHeight;

    public MainWindowViewModel(
        IAppConfigService appConfigService,
        IAppPathsService appPathsService,
        IBgraBitmapConversionService bitmapConversionService,
        IWallpaperComposer wallpaperComposer,
        IScreenInfoService screenInfoService,
        IWallpaperService wallpaperService)
    {
        _appConfigService = appConfigService;
        _appPathsService = appPathsService;
        _bitmapConversionService = bitmapConversionService;
        _wallpaperComposer = wallpaperComposer;
        _screenInfoService = screenInfoService;
        _wallpaperService = wallpaperService;
        _primaryDisplaySize = _screenInfoService.GetPrimaryScreenSize();
        FillModes =
        [
            new(FillModeOption.Cover, "裁剪填充"),
            new(FillModeOption.Contain, "完整显示"),
            new(FillModeOption.Stretch, "拉伸铺满"),
        ];
        PrimaryDisplayResolutionText = $"{_primaryDisplaySize.Width} × {_primaryDisplaySize.Height}";
        PrimaryDisplayAspectText = CalculateAspectText(_primaryDisplaySize.Width, _primaryDisplaySize.Height);
        PrimaryDisplaySummaryText = $"主显示器 · {PrimaryDisplayResolutionText} · {PrimaryDisplayAspectText}";
        RecalculatePreviewSurface();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<FillModeDisplayOption> FillModes { get; }

    public string PrimaryDisplayResolutionText { get; }

    public string PrimaryDisplayAspectText { get; }

    public string PrimaryDisplaySummaryText { get; }

    public string? LeftImagePath
    {
        get => _leftImagePath;
        private set => SetField(ref _leftImagePath, value);
    }

    public string? RightImagePath
    {
        get => _rightImagePath;
        private set => SetField(ref _rightImagePath, value);
    }

    public int SplitPercentage => _splitPercentage;

    public double SplitRatio => _splitPercentage / 100d;

    public double SplitSliderValue
    {
        get => _splitPercentage;
        set => SetSplitPercentage((int)Math.Round(value, MidpointRounding.AwayFromZero));
    }

    public string SplitRatioText => $"{_splitPercentage}%";

    public FillModeOption SelectedFillMode
    {
        get => _selectedFillMode;
        set
        {
            if (SetField(ref _selectedFillMode, value))
            {
                RefreshPreviewIfReady();
            }
        }
    }

    public double PreviewSurfaceWidth
    {
        get => _previewSurfaceWidth;
        private set => SetField(ref _previewSurfaceWidth, value);
    }

    public double PreviewSurfaceHeight
    {
        get => _previewSurfaceHeight;
        private set => SetField(ref _previewSurfaceHeight, value);
    }

    public BitmapSource? PreviewImageSource
    {
        get => _previewImageSource;
        private set => SetField(ref _previewImageSource, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public bool CanApply => !_isBusy && _leftBitmap is not null && _rightBitmap is not null;

    public async Task InitializeAsync()
    {
        try
        {
            var config = await _appConfigService.LoadAsync(_appPathsService.ConfigPath);

            if (config is null)
            {
                StatusMessage = "请选择左右图片开始预览。";
                return;
            }

            _selectedFillMode = config.FillMode;
            OnPropertyChanged(nameof(SelectedFillMode));

            _splitPercentage = LayoutCalculator.NormalizeSplitPercentage(config.SplitRatio);
            OnPropertyChanged(nameof(SplitPercentage));
            OnPropertyChanged(nameof(SplitSliderValue));
            OnPropertyChanged(nameof(SplitRatio));
            OnPropertyChanged(nameof(SplitRatioText));

            if (!string.IsNullOrWhiteSpace(config.LeftImagePath) && File.Exists(config.LeftImagePath))
            {
                var leftImagePath = config.LeftImagePath;
                _leftBitmap = _bitmapConversionService.LoadFromFile(leftImagePath);
                LeftImagePath = leftImagePath;
            }

            if (!string.IsNullOrWhiteSpace(config.RightImagePath) && File.Exists(config.RightImagePath))
            {
                var rightImagePath = config.RightImagePath;
                _rightBitmap = _bitmapConversionService.LoadFromFile(rightImagePath);
                RightImagePath = rightImagePath;
            }

            RefreshPreviewIfReady();
            OnPropertyChanged(nameof(CanApply));
            StatusMessage = CanApply ? "已恢复上次配置。" : "已恢复部分配置，请重新选择缺失图片。";
        }
        catch (Exception exception)
        {
            StatusMessage = $"读取配置失败：{exception.Message}";
        }
    }

    public void UpdatePreviewSize(double width, double height)
    {
        var normalizedWidth = Math.Max(1, width);
        var normalizedHeight = Math.Max(1, height);

        if (Math.Abs(normalizedWidth - _previewHostWidth) < double.Epsilon && Math.Abs(normalizedHeight - _previewHostHeight) < double.Epsilon)
        {
            return;
        }

        _previewHostWidth = normalizedWidth;
        _previewHostHeight = normalizedHeight;
        RecalculatePreviewSurface();
        RefreshPreviewIfReady();
    }

    public void SetSplitRatioFromPreview(double splitRatio)
    {
        SetSplitPercentage(LayoutCalculator.NormalizeSplitPercentage(splitRatio));
    }

    public void SetLeftImagePath(string path)
    {
        LoadImage(path, isLeftImage: true);
    }

    public void SetRightImagePath(string path)
    {
        LoadImage(path, isLeftImage: false);
    }

    public async Task ApplyAsync()
    {
        if (!CanApply)
        {
            StatusMessage = "请先选择左右两张图片。";
            return;
        }

        try
        {
            SetBusy(true);
            StatusMessage = "正在生成最终壁纸...";
            await Task.Yield();

            var screenSize = await Task.Run(() =>
            {
                var currentScreenSize = _screenInfoService.GetPrimaryScreenSize();
                var composedWallpaper = _wallpaperComposer.Compose(_leftBitmap!, _rightBitmap!, currentScreenSize, SplitRatio, SelectedFillMode);
                _bitmapConversionService.SaveAsBmp(composedWallpaper, _appPathsService.GeneratedWallpaperPath);
                _wallpaperService.ApplyWallpaper(_appPathsService.GeneratedWallpaperPath);
                return currentScreenSize;
            });

            await _appConfigService.SaveAsync(_appPathsService.ConfigPath, new AppConfig
            {
                LeftImagePath = LeftImagePath,
                RightImagePath = RightImagePath,
                SplitRatio = SplitRatio,
                FillMode = SelectedFillMode,
            });

            StatusMessage = $"已应用壁纸，输出尺寸 {screenSize.Width} × {screenSize.Height}。";
        }
        catch (Exception exception)
        {
            StatusMessage = $"应用失败：{exception.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void LoadImage(string path, bool isLeftImage)
    {
        try
        {
            var bitmap = _bitmapConversionService.LoadFromFile(path);

            if (isLeftImage)
            {
                _leftBitmap = bitmap;
                LeftImagePath = path;
            }
            else
            {
                _rightBitmap = bitmap;
                RightImagePath = path;
            }

            RefreshPreviewIfReady();
            OnPropertyChanged(nameof(CanApply));

            StatusMessage = CanApply
                ? "图片已加载，可以拖动预览分割线或直接应用壁纸。"
                : "图片已加载，请继续选择另一张。";
        }
        catch (Exception exception)
        {
            StatusMessage = $"加载图片失败：{exception.Message}";
        }
    }

    private void RecalculatePreviewSurface()
    {
        var previewSurface = PreviewLayoutCalculator.CalculateMaxPreviewSize(
            _primaryDisplaySize.Width,
            _primaryDisplaySize.Height,
            _previewHostWidth,
            _previewHostHeight);

        PreviewSurfaceWidth = previewSurface.Width;
        PreviewSurfaceHeight = previewSurface.Height;
    }

    private void RefreshPreviewIfReady()
    {
        if (_leftBitmap is null || _rightBitmap is null || PreviewSurfaceWidth <= 0 || PreviewSurfaceHeight <= 0)
        {
            PreviewImageSource = null;
            return;
        }

        var previewWidth = Math.Max(1, (int)Math.Round(PreviewSurfaceWidth));
        var previewHeight = Math.Max(1, (int)Math.Round(PreviewSurfaceHeight));
        var previewBitmap = _wallpaperComposer.Compose(_leftBitmap, _rightBitmap, new PixelSize(previewWidth, previewHeight), SplitRatio, SelectedFillMode);
        PreviewImageSource = _bitmapConversionService.ToBitmapSource(previewBitmap);
    }

    private void SetBusy(bool isBusy)
    {
        if (SetField(ref _isBusy, isBusy))
        {
            OnPropertyChanged(nameof(CanApply));
        }
    }


    private void SetSplitPercentage(int splitPercentage)
    {
        var normalizedPercentage = LayoutCalculator.NormalizeSplitPercentage(splitPercentage / 100d);

        if (!SetField(ref _splitPercentage, normalizedPercentage, nameof(SplitPercentage)))
        {
            return;
        }

        OnPropertyChanged(nameof(SplitSliderValue));
        OnPropertyChanged(nameof(SplitRatio));
        OnPropertyChanged(nameof(SplitRatioText));
        RefreshPreviewIfReady();
    }

    private static string CalculateAspectText(int width, int height)
    {
        var divisor = GreatestCommonDivisor(width, height);
        return $"{width / divisor}:{height / divisor}";
    }

    private static int GreatestCommonDivisor(int left, int right)
    {
        while (right != 0)
        {
            var remainder = left % right;
            left = right;
            right = remainder;
        }

        return Math.Abs(left);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
