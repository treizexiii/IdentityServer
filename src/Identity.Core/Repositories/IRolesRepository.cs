using Identity.Core.Entities;

namespace Identity.Core.Repositories;

public interface IRolesRepository
{
    Task<IEnumerable<Role>> GetRolesAsync();
    Task<Role?> GetRoleAsync(Guid id);
    Task<Role?> GetRoleAsync(string name);
}