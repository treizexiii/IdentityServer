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

    public async Task<IEnumerable<App>> GetAppsAsync()
    {
        return await context.Apps
            .Include(a => a.Configuration)
            .ToListAsync();
    }

    public async Task<IEnumerable<App>> GetAppsAsync(Guid ownerId)
    {
        return await context.Apps
            .Where(a => a.Owner == ownerId)
            .Include(a => a.Configuration)
            .ToListAsync();
    }

    public async Task<App?> GetAppAsync(Guid id)
    {
        return await context.Apps
            .Where(a => a.Id == id)
            .Include(a => a.Configuration)
            .FirstOrDefaultAsync();
    }

    public async Task<App?> GetAppAsync(string apiKey)
    {
        // try to get app without saving it to the context
        var app = context.Apps.Local.FirstOrDefault(a => a.ApiKey == apiKey);
        if (app is not null)
        {
            return app;
        }
        return await context.Apps
            .Where(a => a.ApiKey == apiKey)
            .Include(a => a.Configuration)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsExistAsync(string name)
    {
        return await context.Apps.AnyAsync(a => a.NormalizedName == name);
    }

    public Task UpdateAppAsync(App app)
    {
        context.Apps.Update(app);
        return Task.CompletedTask;
    }
}