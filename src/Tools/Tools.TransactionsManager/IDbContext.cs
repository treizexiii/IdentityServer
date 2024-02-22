using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Tools.TransactionsManager;

public interface IDbContext
{
    IDbContextTransaction? CurrentTransaction { get; }
    ChangeTracker ChangeTracker { get; }
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
}