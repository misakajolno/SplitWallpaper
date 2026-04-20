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
using System.Windows.Input;
using System.Windows.Media;

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
        UpdateSelectionOverlay();
    }

    private void PreviewViewport_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.UpdatePreviewSize(e.NewSize.Width, e.NewSize.Height);
    }

    private void PreviewHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSplitHandlePosition();
        UpdateSelectionOverlay();
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
        UpdateSelectionOverlay();
    }

    private void PreviewOverlay_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<Thumb>(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        Keyboard.Focus(PreviewHost);
        var point = e.GetPosition(PreviewOverlay);

        if (TryHitPreviewSide(point, PreviewSelectionSide.Left))
        {
            ViewModel.SelectPreviewSide(PreviewSelectionSide.Left);
            UpdateSelectionOverlay();
            e.Handled = true;
            return;
        }

        if (TryHitPreviewSide(point, PreviewSelectionSide.Right))
        {
            ViewModel.SelectPreviewSide(PreviewSelectionSide.Right);
            UpdateSelectionOverlay();
            e.Handled = true;
        }
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        HandlePreviewArrowKey(e);
    }

    private void Window_OnKeyDown(object sender, KeyEventArgs e)
    {
        HandlePreviewArrowKey(e);
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
            or nameof(MainWindowViewModel.PreviewSurfaceHeight)
            or nameof(MainWindowViewModel.SelectedPreviewSide))
        {
            UpdateSplitHandlePosition();
            UpdateSelectionOverlay();
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

    private bool TryNudgeSelectedImage(int deltaX, int deltaY)
    {
        if (!ViewModel.HasSelectedPreview)
        {
            return false;
        }

        Keyboard.Focus(PreviewHost);
        ViewModel.NudgeSelectedImage(deltaX, deltaY);
        UpdateSelectionOverlay();
        return true;
    }

    private void HandlePreviewArrowKey(KeyEventArgs e)
    {
        if (e.Handled || !ViewModel.HasSelectedPreview || !IsArrowKey(e.Key))
        {
            return;
        }

        if (!CanHandlePreviewArrowKey())
        {
            return;
        }

        var step = GetArrowNudgeStep();
        var handled = e.Key switch
        {
            Key.Left => TryNudgeSelectedImage(-step, 0),
            Key.Right => TryNudgeSelectedImage(step, 0),
            Key.Up => TryNudgeSelectedImage(0, -step),
            Key.Down => TryNudgeSelectedImage(0, step),
            _ => false,
        };

        if (handled)
        {
            e.Handled = true;
        }
    }

    private static bool IsArrowKey(Key key)
    {
        return key is Key.Left or Key.Right or Key.Up or Key.Down;
    }

    private static int GetArrowNudgeStep()
    {
        return Keyboard.Modifiers switch
        {
            ModifierKeys.Control => 1,
            ModifierKeys.Shift => 50,
            _ => 10,
        };
    }

    private static bool CanHandlePreviewArrowKey()
    {
        return Keyboard.FocusedElement switch
        {
            TextBoxBase => false,
            ComboBox => false,
            ComboBoxItem => false,
            Slider => false,
            Thumb => false,
            _ => true,
        };
    }

    private bool TryHitPreviewSide(Point point, PreviewSelectionSide side)
    {
        return ViewModel.TryGetPreviewVisibleRect(side, out var visibleRect)
               && point.X >= visibleRect.X
               && point.X <= visibleRect.X + visibleRect.Width
               && point.Y >= visibleRect.Y
               && point.Y <= visibleRect.Y + visibleRect.Height;
    }

    private void UpdateSelectionOverlay()
    {
        if (!IsLoaded || PreviewOverlay.ActualWidth <= 0 || PreviewOverlay.ActualHeight <= 0 || !ViewModel.HasSelectedPreview)
        {
            HideSelectionOverlay();
            return;
        }

        if (!ViewModel.TryGetPreviewVisibleRect(ViewModel.SelectedPreviewSide, out var visibleRect))
        {
            HideSelectionOverlay();
            return;
        }

        SelectionFrame.Visibility = Visibility.Visible;
        SelectionFrame.Width = Math.Max(0, visibleRect.Width);
        SelectionFrame.Height = Math.Max(0, visibleRect.Height);
        Canvas.SetLeft(SelectionFrame, visibleRect.X);
        Canvas.SetTop(SelectionFrame, visibleRect.Y);

        OffsetXText.Text = ViewModel.SelectedOffsetXText;
        OffsetYText.Text = ViewModel.SelectedOffsetYText;
        OffsetXBadge.Visibility = Visibility.Visible;
        OffsetYBadge.Visibility = Visibility.Visible;

        var xBadgeSize = MeasureOverlayElement(OffsetXBadge);
        var yBadgeSize = MeasureOverlayElement(OffsetYBadge);
        var horizontalPadding = 8d;
        var verticalPadding = 8d;
        var isLeftSelected = ViewModel.SelectedPreviewSide == PreviewSelectionSide.Left;
        var anchorX = isLeftSelected ? visibleRect.X + horizontalPadding : visibleRect.X + visibleRect.Width - xBadgeSize.Width - horizontalPadding;
        var bottomX = isLeftSelected ? visibleRect.X + horizontalPadding : visibleRect.X + visibleRect.Width - yBadgeSize.Width - horizontalPadding;
        var centerY = visibleRect.Y + ((visibleRect.Height - xBadgeSize.Height) / 2d);
        var bottomY = visibleRect.Y + visibleRect.Height - yBadgeSize.Height - verticalPadding;

        Canvas.SetLeft(OffsetXBadge, ClampOverlayPosition(anchorX, xBadgeSize.Width, PreviewOverlay.ActualWidth));
        Canvas.SetTop(OffsetXBadge, ClampOverlayPosition(centerY, xBadgeSize.Height, PreviewOverlay.ActualHeight));

        Canvas.SetLeft(OffsetYBadge, ClampOverlayPosition(bottomX, yBadgeSize.Width, PreviewOverlay.ActualWidth));
        Canvas.SetTop(OffsetYBadge, ClampOverlayPosition(bottomY, yBadgeSize.Height, PreviewOverlay.ActualHeight));
    }

    private void HideSelectionOverlay()
    {
        SelectionFrame.Visibility = Visibility.Collapsed;
        OffsetXBadge.Visibility = Visibility.Collapsed;
        OffsetYBadge.Visibility = Visibility.Collapsed;
    }

    private static Size MeasureOverlayElement(FrameworkElement element)
    {
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return element.DesiredSize;
    }

    private static double ClampOverlayPosition(double position, double elementLength, double maxLength)
    {
        return Math.Clamp(position, 0, Math.Max(0, maxLength - elementLength));
    }

    private static T? FindAncestor<T>(DependencyObject? source) where T : DependencyObject
    {
        var current = source;

        while (current is not null)
        {
            if (current is T typed)
            {
                return typed;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
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
