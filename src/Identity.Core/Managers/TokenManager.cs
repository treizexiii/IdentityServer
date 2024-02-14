using Identity.Core.Entities;
using Identity.Core.Repositories;

namespace Identity.Core.Managers;

public interface ITokenManager
{
    Task<UserToken> GenerateTokenAsync(Guid userId, string loginProvider, string name, string value, DateTime? expiration);
    Task<IEnumerable<UserToken>> GetAllTokensAsync(Guid userId);
    Task<UserToken?> GetLastTokenAsync(Guid userId, string loginProvider, string name);
    Task RevokeTokenAsync(UserToken token);
}

internal class TokenManager(ITokenStore<UserToken> tokenStore) : ITokenManager
{
    public Task<UserToken> GenerateTokenAsync(Guid userId, string loginProvider, string name, string value, DateTime? expiration)
    {
        var token = new UserToken
        {
            UserId = userId,
            LoginProvider = loginProvider,
            Name = name,
            Value = value,
            DeletedAt = expiration
        };
        return StoreTokenAsync(token);
    }

    public async Task<IEnumerable<UserToken>> GetAllTokensAsync(Guid userId)
    {
        var query = tokenStore.Where(t =>
            t.UserId == userId &&
            (t.DeletedAt == null || t.DeletedAt > DateTimeOffset.UtcNow));

        return await tokenStore.GetTokenListAsync(query);
    }

    public async Task<UserToken?> GetLastTokenAsync(Guid userId, string loginProvider, string name)
    {
        var query = tokenStore.Where(t =>
            t.UserId == userId &&
            t.LoginProvider == loginProvider &&
            t.Name == name &&
            (t.DeletedAt == null || t.DeletedAt > DateTimeOffset.UtcNow));

        return await tokenStore.GetTokenAsync(query);
    }

    public async Task<UserToken> StoreTokenAsync(UserToken token)
    {
        var currentToken = await GetLastTokenAsync(token.UserId, token.LoginProvider, token.Name);
        if (currentToken is not null)
        {
            await RevokeTokenAsync(currentToken);
        }

        await tokenStore.AddAsync(token);
        return token;
    }

    public async Task RevokeTokenAsync(UserToken token)
    {
        var currentToken = await GetLastTokenAsync(token.UserId, token.LoginProvider, token.Name);
        if (currentToken is not null)
        {
            currentToken.DeletedAt = DateTimeOffset.UtcNow;
            await tokenStore.UpdateAsync(currentToken);
        }
    }
}