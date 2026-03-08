using Microsoft.Win32;
using SplitWallpaper.App.Services;
using SplitWallpaper.App.ViewModels;
using SplitWallpaper.Core.Models;
using SplitWallpaper.Core.Rendering;
using SplitWallpaper.Core.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SplitWallpaper.App;

public partial class MainWindow : Window
{
    private readonly IAppConfigService _appConfigService;
    private readonly IAppPathsService _appPathsService;
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        _appConfigService = new AppConfigService();
        _appPathsService = new AppPathsService();

        InitializeComponent();
        RestoreWindowState(LoadStartupConfig());

        DataContext = new MainWindowViewModel(
            _appConfigService,
            _appPathsService,
            new BgraBitmapConversionService(),
            new WallpaperComposer(),
            new ScreenInfoService(),
            new WallpaperService());

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closing += Window_OnClosing;
    }

    private async void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdatePreviewSize(PreviewViewport.ActualWidth, PreviewViewport.ActualHeight);
        await ViewModel.InitializeAsync();
        UpdateSplitHandlePosition();
    }

    private void PreviewViewport_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.UpdatePreviewSize(e.NewSize.Width, e.NewSize.Height);
    }

    private void PreviewHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSplitHandlePosition();
    }

    private void SplitThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (PreviewOverlay.ActualWidth <= 0)
        {
            return;
        }

        var currentCenter = PreviewOverlay.ActualWidth * ViewModel.SplitRatio;
        var nextCenter = currentCenter + e.HorizontalChange;
        var minimum = PreviewOverlay.ActualWidth * LayoutCalculator.MinimumSplitRatio;
        var maximum = PreviewOverlay.ActualWidth * LayoutCalculator.MaximumSplitRatio;
        nextCenter = Math.Clamp(nextCenter, minimum, maximum);

        ViewModel.SetSplitRatioFromPreview(nextCenter / PreviewOverlay.ActualWidth);
        UpdateSplitHandlePosition();
    }

    private void SelectLeftImage_Click(object sender, RoutedEventArgs e)
    {
        var selectedPath = PickImagePath();

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            ViewModel.SetLeftImagePath(selectedPath);
            UpdateSplitHandlePosition();
        }
    }

    private void SelectRightImage_Click(object sender, RoutedEventArgs e)
    {
        var selectedPath = PickImagePath();

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            ViewModel.SetRightImagePath(selectedPath);
            UpdateSplitHandlePosition();
        }
    }

    private async void ApplyWallpaper_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ApplyAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.SplitRatio)
            or nameof(MainWindowViewModel.PreviewImageSource)
            or nameof(MainWindowViewModel.PreviewSurfaceWidth)
            or nameof(MainWindowViewModel.PreviewSurfaceHeight))
        {
            UpdateSplitHandlePosition();
        }
    }

    private void UpdateSplitHandlePosition()
    {
        if (!IsLoaded || PreviewOverlay.ActualWidth <= 0 || PreviewOverlay.ActualHeight <= 0)
        {
            return;
        }

        var splitCenter = PreviewOverlay.ActualWidth * ViewModel.SplitRatio;
        var maxThumbLeft = Math.Max(0, PreviewOverlay.ActualWidth - SplitThumb.Width);
        var maxLineLeft = Math.Max(0, PreviewOverlay.ActualWidth - SplitLine.Width);

        Canvas.SetLeft(SplitThumb, Math.Clamp(splitCenter - (SplitThumb.Width / 2d), 0, maxThumbLeft));
        Canvas.SetTop(SplitThumb, 0);
        SplitThumb.Height = PreviewOverlay.ActualHeight;

        Canvas.SetLeft(SplitLine, Math.Clamp(splitCenter - (SplitLine.Width / 2d), 0, maxLineLeft));
        Canvas.SetTop(SplitLine, 0);
        SplitLine.Height = PreviewOverlay.ActualHeight;
    }

    private static string? PickImagePath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择壁纸图片",
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private AppConfig? LoadStartupConfig()
    {
        try
        {
            return Task.Run(() => _appConfigService.LoadAsync(_appPathsService.ConfigPath)).GetAwaiter().GetResult();
        }
        catch
        {
            return null;
        }
    }

    private void RestoreWindowState(AppConfig? config)
    {
        var visibleBounds = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        if (!WindowStatePersistence.TryRestore(config, MinWidth, MinHeight, visibleBounds, out var restoredState))
        {
            return;
        }

        Left = restoredState!.Left;
        Top = restoredState.Top;
        Width = restoredState.Width;
        Height = restoredState.Height;

        if (restoredState.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        try
        {
            var config = Task.Run(() => _appConfigService.LoadAsync(_appPathsService.ConfigPath)).GetAwaiter().GetResult()
                ?? new AppConfig();

            var capturedState = WindowStatePersistence.Capture(
                Left,
                Top,
                Width,
                Height,
                RestoreBounds,
                WindowState,
                MinWidth,
                MinHeight);

            config.WindowLeft = capturedState.Left;
            config.WindowTop = capturedState.Top;
            config.WindowWidth = capturedState.Width;
            config.WindowHeight = capturedState.Height;
            config.IsWindowMaximized = capturedState.IsMaximized;

            Task.Run(() => _appConfigService.SaveAsync(_appPathsService.ConfigPath, config)).GetAwaiter().GetResult();
        }
        catch
        {
        }
    }
}
