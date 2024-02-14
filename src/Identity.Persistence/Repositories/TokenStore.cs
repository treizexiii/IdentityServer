using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class TokenStore(IdentityDb context) : ITokenStore<UserToken>
{
    public IQueryable<UserToken> Where(Func<UserToken, bool> func)
    {
        return context.UserTokens.Where(func).AsQueryable();
    }

    public async Task<IEnumerable<UserToken>> GetTokenListAsync(IQueryable<UserToken> query)
    {
        return await query.ToListAsync();
    }

    public Task<UserToken?> GetTokenAsync(IQueryable<UserToken> query)
    {
        return query.FirstOrDefaultAsync();
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
