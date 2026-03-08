using SplitWallpaper.Core.Models;

namespace SplitWallpaper.App.Services;

public interface IScreenInfoService
{
    PixelSize GetPrimaryScreenSize();
}
