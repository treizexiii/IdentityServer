using Identity.Core.Entities;

namespace Identity.Core.Repositories;

public interface ITokenStore<TUserToken> where TUserToken : UserToken
{
    IQueryable<TUserToken> Where(Func<TUserToken, bool> func);
    Task<IEnumerable<UserToken>> GetTokenListAsync(IQueryable<UserToken> query);
    Task<UserToken?> GetTokenAsync(IQueryable<UserToken> query);
    Task AddAsync(TUserToken token);
    Task UpdateAsync(UserToken currentToken);
}