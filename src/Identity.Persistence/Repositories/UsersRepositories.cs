using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class UsersRepositories(IdentityDb context) : IUsersRepository, IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetUserAsync(Guid id)
    {
        return await context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserAsync(string username)
    {
        return await context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
    }

    public Task<bool> IsExistAsync(string username)
    {
        return context.Users.AnyAsync(u => u.UserName == username);
    }

    public async Task AddUserAsync(User user)
    {
        await context.Users.AddAsync(user);
    }

    public async Task<UserToken?> GetLastTokenAsync(Guid userId, string @internal, string refreshToken)
    {
        return await context.UserTokens.Where(t =>
                t.UserId == userId &&
                t.LoginProvider == @internal &&
                t.Name == refreshToken)
            .FirstOrDefaultAsync();
    }

    public async Task AddTokenAsync(UserToken token)
    {
        var tokens = await context.UserTokens.Where(t =>
                t.UserId == token.UserId &&
                t.LoginProvider == token.LoginProvider &&
                t.Name == token.Name)
            .FirstOrDefaultAsync();
        if (tokens is not null) context.UserTokens.Remove(tokens);
        await context.UserTokens.AddAsync(token);
    }

    public Task RemoveTokenAsync(UserToken token)
    {
        context.UserTokens.Remove(token);
        return Task.CompletedTask;
    }

    public async Task<Role?> GetRoleAsync(string role)
    {
        return await context.Roles.Where(r => r.Name == role).FirstOrDefaultAsync();
    }

    public async Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        var current = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
        return current?.Id.ToString() ?? string.Empty;
    }

    public async Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        var current = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
        return current?.UserName;
    }

    public async Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        var current = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
        if (current is not null)
        {
            current.UserName = userName;
            context.Users.Update(current);
        }
    }

    public async Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        var current = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
        return current?.NormalizedUserName;
    }

    public async Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        var current = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
        if (current is null)
            throw new InvalidOperationException("User not found");
        current.NormalizedUserName = normalizedName;
        context.Users.Update(current);
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
        return IdentityResult.Success;
    }

    public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        context.Users.Update(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        var current = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);
        if (current is null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        user.DeactivatedAt = DateTime.UtcNow;
        user.Active = false;
        user.UserName = $"{user.UserName}-{user.Id}";
        user.NormalizedUserName = $"{user.NormalizedUserName}-{user.Id}";
        context.Users.Update(user);
        return IdentityResult.Success;
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var id = Guid.Parse(userId);
        return await context.Users.Where(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await context.Users.Where(u => u.NormalizedUserName == normalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Dispose()
    {
        context.Dispose();
    }
}