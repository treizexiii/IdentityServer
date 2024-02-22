using Microsoft.Extensions.Logging;
using Tools.TransactionsManager.Data;

namespace Tools.TransactionsManager.Managers;

public class MultiContextTransactionManager(
    ILogger<MultiContextTransactionManager> logger,
    IEnumerable<IDbContext> contexts)
    : ITransactionManager
{
    private readonly List<TransactionInfo> _transactionList = [];

    public Task BeginTransactionAsync(Guid userId)
    {
        var currentContext = contexts.ToList();
        return Begin(userId, currentContext);
    }

    public Task<TransactionInfo> CommitTransactionAsync(Guid userId)
    {
        var currentContext = contexts.ToList();
        return Commit(userId, currentContext);
    }

    public Task<TransactionInfo> RollbackTransactionAsync(Guid userId, Exception e)
    {
        var currentContext = contexts.ToList();
        return Rollback(userId, e, currentContext);
    }

    public Task<TransactionInfo> RollbackTransactionAsync(Guid userId, string message)
    {
        var currentContext = contexts.ToList();
        return Rollback(userId, message, currentContext);
    }

    public async Task BeginTransactionAsync(Guid userId, params Type[] contextName)
    {
        var currentContext = contextName.Select(GetContext).ToList();
        await Begin(userId, currentContext);
    }

    private async Task Begin(Guid userId, List<IDbContext> currentContext)
    {
        var transaction = new TransactionInfo(userId);
        foreach (var db in currentContext)
        {
            var newTransaction = await db.BeginTransactionAsync();
            transaction.AddTransaction(db, newTransaction);
        }

        _transactionList.Add(transaction);

        logger.LogInformation("Transaction started for user {UserId}", userId);
    }

    public async Task<TransactionInfo> CommitTransactionAsync(Guid userId, params Type[] contextName)
    {
        var currentContext = contextName.Select(GetContext).ToList();
        return await Commit(userId, currentContext);
    }

    private async Task<TransactionInfo> Commit(Guid userId, List<IDbContext> currentContext)
    {
        var transactionInfo = _transactionList.FirstOrDefault(x => x.UserId == userId);
        if (transactionInfo is null) throw new ArgumentException($"Transaction for user {userId} not found");

        try
        {
            foreach (var transaction in transactionInfo.Context)
            {
                var db = currentContext.FirstOrDefault(x => x.GetType().Name == transaction.Key);
                if (db is null) throw new ArgumentException($"Context {transaction.Key} not found");

                var currentTransaction = db.CurrentTransaction;
                if (currentTransaction is null || currentTransaction.TransactionId != transaction.Value.TransactionId)
                    throw new ArgumentException($"Transaction for context {transaction.Key} not current");

                await db.SaveChangesAsync();
                await currentTransaction.CommitAsync();
                logger.LogInformation("Transaction commit for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            transactionInfo.SetStatus(TransactionStatusEnum.Committed);
        }
        catch (Exception e)
        {
            logger.LogError("Error while commit transaction: {Message}", e.Message);
            foreach (var transaction in transactionInfo.Context)
            {
                await transaction.Value.RollbackAsync();
                logger.LogInformation("Transaction rollback for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            transactionInfo.SetMessage(e.Message);
            transactionInfo.SetStatus(TransactionStatusEnum.Failed);
        }
        finally
        {
            foreach (var transaction in transactionInfo.Context) await transaction.Value.DisposeAsync();

            _transactionList.Remove(transactionInfo);

            logger.LogInformation("Transactions end for user {UserId}", userId);
        }

        return transactionInfo;
    }

    public async Task<TransactionInfo> RollbackTransactionAsync(Guid userId, Exception e, params Type[] contextName)
    {
        var currentContext = contextName.Select(GetContext).ToList();
        return await Rollback(userId, e, currentContext);
    }

    private async Task<TransactionInfo> Rollback(Guid userId, Exception e,
        IReadOnlyCollection<IDbContext> currentContext)
    {
        var transactionInfo = _transactionList.FirstOrDefault(x => x.UserId == userId);
        if (transactionInfo is null) throw new ArgumentException($"Transaction for user {userId} not found");

        try
        {
            foreach (var transaction in transactionInfo.Context)
            {
                var db = currentContext.FirstOrDefault(x => x.GetType().Name == transaction.Key);
                if (db is null) throw new ArgumentException($"Context {transaction.Key} not found");

                var currentTransaction = db.CurrentTransaction;
                if (currentTransaction is null || currentTransaction.TransactionId != transaction.Value.TransactionId)
                    throw new ArgumentException($"Transaction for context {transaction.Key} not current");

                db.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                await currentTransaction.RollbackAsync();
                logger.LogInformation("Transaction rollback for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            transactionInfo.SetMessage(e.Message);
            transactionInfo.SetStatus(TransactionStatusEnum.RolledBack);
        }
        catch (Exception exception)
        {
            logger.LogError("Error while rollback transaction: {Message}", exception.Message);
            transactionInfo.SetMessage(exception.Message);
            transactionInfo.SetStatus(TransactionStatusEnum.Failed);
            throw;
        }
        finally
        {
            foreach (var transaction in transactionInfo.Context) transaction.Value.Dispose();

            _transactionList.Remove(transactionInfo);

            logger.LogInformation("Transactions end for user {UserId}", userId);
        }

        return transactionInfo;
    }

    private async Task<TransactionInfo> Rollback(Guid userId, string message,
        IReadOnlyCollection<IDbContext> currentContext)
    {
        var transactionInfo = _transactionList.FirstOrDefault(x => x.UserId == userId);
        if (transactionInfo is null) throw new ArgumentException($"Transaction for user {userId} not found");

        try
        {
            foreach (var transaction in transactionInfo.Context)
            {
                var db = currentContext.FirstOrDefault(x => x.GetType().Name == transaction.Key);
                if (db is null) throw new ArgumentException($"Context {transaction.Key} not found");

                var currentTransaction = db.CurrentTransaction;
                if (currentTransaction is null || currentTransaction.TransactionId != transaction.Value.TransactionId)
                    throw new ArgumentException($"Transaction for context {transaction.Key} not current");

                db.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                await currentTransaction.RollbackAsync();
                logger.LogInformation("Transaction rollback for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            transactionInfo.SetMessage(message);
            transactionInfo.SetStatus(TransactionStatusEnum.RolledBack);
        }
        catch (Exception exception)
        {
            logger.LogError("Error while rollback transaction: {Message}", exception.Message);
            transactionInfo.SetMessage(exception.Message);
            transactionInfo.SetStatus(TransactionStatusEnum.Failed);
            throw;
        }
        finally
        {
            foreach (var transaction in transactionInfo.Context) transaction.Value.Dispose();

            _transactionList.Remove(transactionInfo);

            logger.LogInformation("Transactions end for user {UserId}", userId);
        }

        return transactionInfo;
    }

    private IDbContext GetContext(Type contextName)
    {
        var context = contexts.FirstOrDefault(x => x.GetType() == contextName);
        if (context is null) throw new ArgumentException($"DbContext {contextName} not found");

        return context;
    }
}