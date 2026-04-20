namespace SplitWallpaper.Core.Models;

public readonly record struct ImageOffset(int X, int Y)
{
    public static ImageOffset Zero => new(0, 0);

    public ImageOffset Translate(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);
}
