using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class RolesRepository(IdentityDb context) : IRolesRepository
{
    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        return await context.Roles.ToListAsync();
    }

    public async Task<Role?> GetRoleAsync(Guid id)
    {
        return await context.Roles.Where(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Role?> GetRoleAsync(string name)
    {
        return await context.Roles.Where(r => r.Name == name).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesAsync(Guid appId)
    {
        return await context.Roles
            .Where(r => r.AppId == appId || r.IsSystemDefault)
            .ToListAsync();
    }

    public async Task<int> DeleteRoleAsync(Guid id)
    {
        var role = await context.Roles
            .Where(r => r.Id == id && r.CouldBeDeleted && !r.IsSystemDefault)
            .FirstOrDefaultAsync();
        if (role is not null)
        {
            context.Roles.Remove(role);
            return 0;
        }

        return -1;
    }

    public Task AddRoleAsync(Role newRole)
    {
        context.Roles.Add(newRole);
        return Task.CompletedTask;
    }
}