namespace SplitWallpaper.Core.Services;

public interface IAppConfigService
{
    Task SaveAsync(string path, SplitWallpaper.Core.Models.AppConfig config, CancellationToken cancellationToken = default);

    Task<SplitWallpaper.Core.Models.AppConfig?> LoadAsync(string path, CancellationToken cancellationToken = default);
}
