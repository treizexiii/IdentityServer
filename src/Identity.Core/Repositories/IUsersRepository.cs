using Identity.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Repositories;

public interface IUsersRepository : IUserStore<User>
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User?> GetUserAsync(Guid id);
    Task<User?> GetUserAsync(string username);
    Task<bool> IsExistAsync(string username);
    Task AddUserAsync(User user);
    Task<UserToken?> GetLastTokenAsync(Guid userId, string @internal, string refreshToken);
    Task AddTokenAsync(UserToken token);
    Task RemoveTokenAsync(UserToken token);
    Task<Role?> GetRoleAsync(string role);
}