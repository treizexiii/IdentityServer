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
}