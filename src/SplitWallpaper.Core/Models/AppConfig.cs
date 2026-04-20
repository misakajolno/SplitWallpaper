namespace SplitWallpaper.Core.Models;

public sealed class AppConfig
{
    public string? LeftImagePath { get; set; }

    public string? RightImagePath { get; set; }

    public double SplitRatio { get; set; } = 0.5;

    public FillModeOption FillMode { get; set; } = FillModeOption.Cover;

    public int LeftOffsetX { get; set; }

    public int LeftOffsetY { get; set; }

    public int RightOffsetX { get; set; }

    public int RightOffsetY { get; set; }

    public double? WindowWidth { get; set; }

    public double? WindowHeight { get; set; }

    public double? WindowLeft { get; set; }

    public double? WindowTop { get; set; }

    public bool IsWindowMaximized { get; set; }
}
