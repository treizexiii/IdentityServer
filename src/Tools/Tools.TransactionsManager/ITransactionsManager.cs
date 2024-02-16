using Tools.TransactionsManager.Data;

namespace Tools.TransactionsManager;

public interface ITransactionManager
{
    Task BeginTransactionAsync(Guid userId);
    Task<TransactionInfo> CommitTransactionAsync(Guid userId);
    Task<TransactionInfo> RollbackTransactionAsync(Guid userId, Exception e);
    Task<TransactionInfo> RollbackTransactionAsync(Guid userId, string message);

    // Task BeginTransactionAsync(Guid userId, params Type[] contextName);
    // Task<TransactionInfo> CommitTransactionAsync(Guid userId, params Type[] contextName);
    // Task<TransactionInfo> RollbackTransactionAsync(Guid userId, Exception e, params Type[] contextName);
}