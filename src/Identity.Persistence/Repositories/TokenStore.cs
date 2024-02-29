using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;

namespace Identity.Persistence.Repositories;

public class TokenStore(IdentityDb context) : ITokenStore<UserToken>
{
    public IQueryable<UserToken> Where(Func<UserToken, bool> func)
    {
        return context.UserTokens.Where(func).AsQueryable();
    }

    public Task<IEnumerable<UserToken>> GetTokenListAsync(IQueryable<UserToken> query)
    {
        var result = query.ToList();
        return Task.FromResult(result as IEnumerable<UserToken>);
    }

    public Task<UserToken?> GetTokenAsync(IQueryable<UserToken> query)
    {
        var result = query.FirstOrDefault();
        return Task.FromResult(result);
    }

    public async Task AddAsync(UserToken token)
    {
        await context.UserTokens.AddAsync(token);
    }

    public Task UpdateAsync(UserToken currentToken)
    {
        context.UserTokens.Update(currentToken);
        return Task.CompletedTask;
    }
}