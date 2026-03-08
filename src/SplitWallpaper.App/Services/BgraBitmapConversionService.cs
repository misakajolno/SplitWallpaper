using SplitWallpaper.Core.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace SplitWallpaper.App.Services;

public sealed class BgraBitmapConversionService : IBgraBitmapConversionService
{
    public BgraBitmap LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Image file not found.", fullPath);
        }

        using var stream = File.OpenRead(fullPath);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        BitmapSource source = decoder.Frames[0];

        if (source.Format != System.Windows.Media.PixelFormats.Bgra32)
        {
            var converted = new FormatConvertedBitmap();
            converted.BeginInit();
            converted.Source = source;
            converted.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
            converted.EndInit();
            converted.Freeze();
            source = converted;
        }

        var stride = source.PixelWidth * 4;
        var pixels = new byte[stride * source.PixelHeight];
        source.CopyPixels(pixels, stride, 0);

        return new BgraBitmap(source.PixelWidth, source.PixelHeight, pixels);
    }

    public BitmapSource ToBitmapSource(BgraBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        var source = BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, bitmap.Pixels, bitmap.Stride);
        source.Freeze();
        return source;
    }

    public void SaveAsPng(BgraBitmap bitmap, string path)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(ToBitmapSource(bitmap)));

        using var stream = File.Create(fullPath);
        encoder.Save(stream);
    }
}
