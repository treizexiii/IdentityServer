using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Tools.TransactionsManager;

public interface IDbContext
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
    IDbContextTransaction? CurrentTransaction { get; }
    ChangeTracker ChangeTracker { get; }
}