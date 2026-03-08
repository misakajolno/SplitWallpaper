using System.IO;
using System.Runtime.InteropServices;

namespace SplitWallpaper.App.Services;

public sealed class WallpaperService : IWallpaperService
{
    private const uint SetDesktopWallpaperAction = 0x0014;
    private const uint UpdateIniFileFlag = 0x01;
    private const uint SendWinIniChangeFlag = 0x02;

    public void ApplyWallpaper(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Generated wallpaper file not found.", fullPath);
        }

        var result = SystemParametersInfo(SetDesktopWallpaperAction, 0, fullPath, UpdateIniFileFlag | SendWinIniChangeFlag);

        if (!result)
        {
            throw new InvalidOperationException($"Windows 壁纸设置失败，错误码: {Marshal.GetLastWin32Error()}。");
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, string value, uint initializationFlags);
}
