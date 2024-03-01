using Identity.Core.Entities;

namespace Identity.Core.Repositories;

public interface IAppsRepository
{
    Task AddAppAsync(App app);
    Task<App?> GetAppAsync(Guid id);
    Task<App?> GetAppAsync(string apiKey);
    Task<bool> IsExistAsync(string name);
    Task UpdateAppAsync(App app);
}