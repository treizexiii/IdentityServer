using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class AppsRepository(IdentityDb context) : IAppsRepository
{
    public async Task AddAppAsync(App app)
    {
        await context.Apps.AddAsync(app);
    }

    public async Task<App?> GetAppAsync(Guid id)
    {
        return await context.Apps.Where(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<App?> GetAppAsync(string key)
    {
        return await context.Apps.Where(a => a.Key == key).FirstOrDefaultAsync();
    }

    public async Task<bool> IsExistAsync(string name)
    {
        return await context.Apps.AnyAsync(a => a.NormalizedName == name);
    }
}