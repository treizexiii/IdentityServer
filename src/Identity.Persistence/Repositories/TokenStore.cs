using System.Linq.Expressions;
using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Identity.Persistence.Repositories;

public class TokenStore(IdentityDb context) : ITokenStore<UserToken>, IAsyncQueryProvider
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

    public IQueryable CreateQuery(Expression expression)
    {
        return new EntityQueryable<UserToken>(this, expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new EntityQueryable<TElement>(this, expression);
    }

    public object? Execute(Expression expression)
    {
        return ((IQueryable)context.UserTokens).Provider.Execute(expression);

    }

    public TResult Execute<TResult>(Expression expression)
    {
        return ((IQueryable)context.UserTokens).Provider.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new CancellationToken())
    {
        return ((IAsyncQueryProvider)context.UserTokens).ExecuteAsync<TResult>(expression, cancellationToken);
    }
}
