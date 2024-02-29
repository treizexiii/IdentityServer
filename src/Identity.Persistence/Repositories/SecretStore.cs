using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;

namespace Identity.Persistence.Repositories;

public class SecretStore(IdentityDb context) : ISecretStore<Secret>
{
    public IQueryable<Secret> Where(Func<Secret, bool> func)
    {
        return context.Secrets.Where(func).AsQueryable();
    }

    public Task<IEnumerable<Secret>> GetTokenListAsync(IQueryable<Secret> query)
    {
        var result = query.ToList();
        return Task.FromResult(result as IEnumerable<Secret>);
    }

    public Task<Secret?> GetTokenAsync(IQueryable<Secret> query)
    {
        var result = query.FirstOrDefault();
        return Task.FromResult(result);
    }

    public async Task AddAsync(Secret token)
    {
        await context.Secrets.AddAsync(token);
    }

    public Task UpdateAsync(Secret currentToken)
    {
        context.Secrets.Update(currentToken);
        return Task.CompletedTask;
    }
}