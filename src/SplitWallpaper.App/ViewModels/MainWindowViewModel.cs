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
    private ImageOffset _leftOffset = ImageOffset.Zero;
    private ImageOffset _rightOffset = ImageOffset.Zero;
    private int _splitPercentage = 50;
    private FillModeOption _selectedFillMode = FillModeOption.Cover;
    private PreviewSelectionSide _selectedPreviewSide;
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

    public PreviewSelectionSide SelectedPreviewSide
    {
        get => _selectedPreviewSide;
        private set
        {
            if (SetField(ref _selectedPreviewSide, value))
            {
                OnPropertyChanged(nameof(HasSelectedPreview));
                OnPropertyChanged(nameof(SelectedPreviewSummaryText));
                OnPropertyChanged(nameof(SelectedOffsetXText));
                OnPropertyChanged(nameof(SelectedOffsetYText));
            }
        }
    }

    public bool HasSelectedPreview => SelectedPreviewSide is not PreviewSelectionSide.None;

    public FillModeOption SelectedFillMode
    {
        get => _selectedFillMode;
        set
        {
            if (SetField(ref _selectedFillMode, value))
            {
                ClampOffsetsToCurrentLayout();
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

    public string SelectedOffsetXText => $"X {FormatOffset(GetSelectedOffset().X)} px";

    public string SelectedOffsetYText => $"Y {FormatOffset(GetSelectedOffset().Y)} px";

    public string SelectedPreviewSummaryText => SelectedPreviewSide switch
    {
        PreviewSelectionSide.Left => "已选中：左图",
        PreviewSelectionSide.Right => "已选中：右图",
        _ => "已选中：未选择",
    };

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
                _leftOffset = new ImageOffset(config.LeftOffsetX, config.LeftOffsetY);
            }

            if (!string.IsNullOrWhiteSpace(config.RightImagePath) && File.Exists(config.RightImagePath))
            {
                var rightImagePath = config.RightImagePath;
                _rightBitmap = _bitmapConversionService.LoadFromFile(rightImagePath);
                RightImagePath = rightImagePath;
                _rightOffset = new ImageOffset(config.RightOffsetX, config.RightOffsetY);
            }

            ClampOffsetsToCurrentLayout();
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

    public void SelectPreviewSide(PreviewSelectionSide side)
    {
        if (!CanSelectPreviewSide(side))
        {
            return;
        }

        SelectedPreviewSide = side;
        StatusMessage = side == PreviewSelectionSide.Left
            ? "已选中左图，可用方向键微调偏移。"
            : "已选中右图，可用方向键微调偏移。";
    }

    public void NudgeSelectedImage(int deltaX, int deltaY)
    {
        if (SelectedPreviewSide is PreviewSelectionSide.None)
        {
            return;
        }

        var currentOffset = GetSelectedOffset();
        var requestedOffset = currentOffset.Translate(deltaX, deltaY);
        ApplyOffsetForSide(SelectedPreviewSide, requestedOffset);
        var latestOffset = GetSelectedOffset();
        var selectionName = GetSelectedPreviewDisplayName(SelectedPreviewSide);

        if (latestOffset == currentOffset)
        {
            if (requestedOffset != currentOffset)
            {
                StatusMessage = $"{selectionName}已到可移动边界，当前偏移：{SelectedOffsetXText}，{SelectedOffsetYText}。";
            }

            return;
        }

        StatusMessage = $"{selectionName}偏移已调整：{SelectedOffsetXText}，{SelectedOffsetYText}。";
    }

    public bool TryGetPreviewVisibleRect(PreviewSelectionSide side, out DoubleRect visibleRect)
    {
        visibleRect = default;

        if (!TryGetPreviewPlacement(side, out _, out visibleRect))
        {
            return false;
        }

        return visibleRect.Width > 0 && visibleRect.Height > 0;
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
                var composedWallpaper = _wallpaperComposer.Compose(_leftBitmap!, _rightBitmap!, currentScreenSize, SplitRatio, SelectedFillMode, _leftOffset, _rightOffset);
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
                LeftOffsetX = _leftOffset.X,
                LeftOffsetY = _leftOffset.Y,
                RightOffsetX = _rightOffset.X,
                RightOffsetY = _rightOffset.Y,
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
                _leftOffset = ImageOffset.Zero;
                LeftImagePath = path;
            }
            else
            {
                _rightBitmap = bitmap;
                _rightOffset = ImageOffset.Zero;
                RightImagePath = path;
            }

            ClampOffsetsToCurrentLayout();
            RefreshPreviewIfReady();
            OnPropertyChanged(nameof(CanApply));
            OnPropertyChanged(nameof(SelectedOffsetXText));
            OnPropertyChanged(nameof(SelectedOffsetYText));

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
        var previewBitmap = _wallpaperComposer.Compose(_leftBitmap, _rightBitmap, new PixelSize(previewWidth, previewHeight), SplitRatio, SelectedFillMode, _leftOffset, _rightOffset);
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
        ClampOffsetsToCurrentLayout();
        RefreshPreviewIfReady();
    }

    private void ApplyOffsetForSide(PreviewSelectionSide side, ImageOffset requestedOffset)
    {
        if (!TryClampOffset(side, requestedOffset, out var clampedOffset))
        {
            return;
        }

        switch (side)
        {
            case PreviewSelectionSide.Left:
                if (!SetField(ref _leftOffset, clampedOffset, nameof(_leftOffset)))
                {
                    return;
                }

                break;
            case PreviewSelectionSide.Right:
                if (!SetField(ref _rightOffset, clampedOffset, nameof(_rightOffset)))
                {
                    return;
                }

                break;
            default:
                return;
        }

        OnPropertyChanged(nameof(SelectedOffsetXText));
        OnPropertyChanged(nameof(SelectedOffsetYText));
        RefreshPreviewIfReady();
    }

    private void ClampOffsetsToCurrentLayout()
    {
        ClampOffsetForSide(PreviewSelectionSide.Left);
        ClampOffsetForSide(PreviewSelectionSide.Right);
        OnPropertyChanged(nameof(SelectedOffsetXText));
        OnPropertyChanged(nameof(SelectedOffsetYText));
    }

    private void ClampOffsetForSide(PreviewSelectionSide side)
    {
        var currentOffset = side == PreviewSelectionSide.Left ? _leftOffset : _rightOffset;

        if (!TryClampOffset(side, currentOffset, out var clampedOffset))
        {
            return;
        }

        if (side == PreviewSelectionSide.Left)
        {
            _leftOffset = clampedOffset;
        }
        else if (side == PreviewSelectionSide.Right)
        {
            _rightOffset = clampedOffset;
        }
    }

    private bool TryClampOffset(PreviewSelectionSide side, ImageOffset requestedOffset, out ImageOffset clampedOffset)
    {
        clampedOffset = ImageOffset.Zero;

        if (!TryGetScreenLayout(side, out var bitmap, out var targetRegion))
        {
            return false;
        }

        clampedOffset = LayoutCalculator.ClampImageOffset(
            new PixelSize(bitmap.Width, bitmap.Height),
            targetRegion,
            SelectedFillMode,
            requestedOffset);

        return true;
    }

    private bool TryGetPreviewPlacement(PreviewSelectionSide side, out DoubleRect placement, out DoubleRect visibleRect)
    {
        placement = default;
        visibleRect = default;

        if (PreviewSurfaceWidth <= 0 || PreviewSurfaceHeight <= 0)
        {
            return false;
        }

        if (!TryGetTargetRegion(
                new PixelSize(Math.Max(1, (int)Math.Round(PreviewSurfaceWidth)), Math.Max(1, (int)Math.Round(PreviewSurfaceHeight))),
                side,
                out var bitmap,
                out var targetRegion))
        {
            return false;
        }

        var offset = side == PreviewSelectionSide.Left ? _leftOffset : _rightOffset;
        placement = LayoutCalculator.CalculatePlacement(new PixelSize(bitmap.Width, bitmap.Height), targetRegion, SelectedFillMode, offset);
        visibleRect = LayoutCalculator.CalculateVisibleRect(placement, targetRegion);
        return true;
    }

    private bool TryGetScreenLayout(PreviewSelectionSide side, out BgraBitmap bitmap, out PixelRect targetRegion)
    {
        return TryGetTargetRegion(_primaryDisplaySize, side, out bitmap, out targetRegion);
    }

    private bool TryGetTargetRegion(PixelSize totalSize, PreviewSelectionSide side, out BgraBitmap bitmap, out PixelRect targetRegion)
    {
        bitmap = null!;
        targetRegion = default;

        if (side == PreviewSelectionSide.Left && _leftBitmap is not null)
        {
            var regions = LayoutCalculator.CalculateRegions(totalSize.Width, totalSize.Height, SplitRatio);
            bitmap = _leftBitmap;
            targetRegion = regions.Left;
            return true;
        }

        if (side == PreviewSelectionSide.Right && _rightBitmap is not null)
        {
            var regions = LayoutCalculator.CalculateRegions(totalSize.Width, totalSize.Height, SplitRatio);
            bitmap = _rightBitmap;
            targetRegion = regions.Right;
            return true;
        }

        return false;
    }

    private bool CanSelectPreviewSide(PreviewSelectionSide side)
    {
        return side switch
        {
            PreviewSelectionSide.Left => _leftBitmap is not null,
            PreviewSelectionSide.Right => _rightBitmap is not null,
            _ => false,
        };
    }

    private ImageOffset GetSelectedOffset()
    {
        return SelectedPreviewSide switch
        {
            PreviewSelectionSide.Left => _leftOffset,
            PreviewSelectionSide.Right => _rightOffset,
            _ => ImageOffset.Zero,
        };
    }

    private static string FormatOffset(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static string GetSelectedPreviewDisplayName(PreviewSelectionSide side)
    {
        return side == PreviewSelectionSide.Left ? "左图" : "右图";
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
