using Identity.Core.Entities;

namespace Identity.Core.Repositories;

public interface ISecretStore<TSecret> where TSecret : Secret
{
    IQueryable<TSecret> Where(Func<TSecret, bool> func);
    Task<IEnumerable<TSecret>> GetTokenListAsync(IQueryable<TSecret> query);
    Task<TSecret?> GetTokenAsync(IQueryable<TSecret> query);
    Task AddAsync(TSecret token);
    Task UpdateAsync(TSecret currentToken);
}