using SplitWallpaper.Core.Models;
using System.Windows.Media.Imaging;

namespace SplitWallpaper.App.Services;

public interface IBgraBitmapConversionService
{
    BgraBitmap LoadFromFile(string path);

    BitmapSource ToBitmapSource(BgraBitmap bitmap);

    void SaveAsPng(BgraBitmap bitmap, string path);
}
