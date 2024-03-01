using Identity.Core.Entities;

namespace Identity.Core.Repositories;

public interface IAppsRepository
{
    Task<IEnumerable<App>> GetAppsAsync();
    Task<IEnumerable<App>> GetAppsAsync(Guid ownerId);
    Task<App?> GetAppAsync(Guid id);
    Task<App?> GetAppAsync(string apiKey);
    Task AddAppAsync(App app);
    Task UpdateAppAsync(App app);
    Task<bool> IsExistAsync(string name);
    Task<IEnumerable<User>> GetAppUsersAsync(Guid id, string role);
}