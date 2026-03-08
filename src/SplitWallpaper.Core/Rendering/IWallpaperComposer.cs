using SplitWallpaper.Core.Models;

namespace SplitWallpaper.Core.Rendering;

public interface IWallpaperComposer
{
    BgraBitmap Compose(BgraBitmap left, BgraBitmap right, PixelSize targetSize, double splitRatio, FillModeOption fillMode);
}
