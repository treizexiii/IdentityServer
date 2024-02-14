using System.Data.Common;
using Microsoft.Extensions.Logging;
using Tools.TransactionsManager.Data;

namespace Tools.TransactionsManager.Managers;

public class TransactionManager(ILogger<TransactionManager> logger, IDbContext context) : ITransactionManager
{
    private TransactionInfo? _transaction;

    public async Task BeginTransactionAsync(Guid userId)
    {
        var transaction = new TransactionInfo(userId);
        try
        {
            var newTransaction = await context.BeginTransactionAsync();
            transaction.AddTransaction(context, newTransaction);
            _transaction = transaction;
            logger.LogInformation("Transaction started for user {UserId}", userId);
        }
        catch (DbException e)
        {
            logger.LogWarning("Error while starting transaction for user {UserId}", userId);
            transaction.HasFailed();
            transaction.SetMessage(e.Message);
            throw new Exception($"Error while starting transaction for user {userId}");
        }
        catch (Exception e)
        {
            logger.LogWarning("Error while starting transaction: {Message}", e.Message);
            transaction.HasFailed();
            throw;
        }
        finally
        {
            _transaction = transaction;
        }
    }

    public async Task<TransactionInfo> CommitTransactionAsync(Guid userId)
    {
        if (_transaction is null)
        {
            throw new ArgumentException($"Transaction for user {userId} not found");
        }

        TransactionInfo currentInfo;
        try
        {
            foreach (var transaction in _transaction.Context)
            {
                if (transaction.Value.TransactionId != context.CurrentTransaction?.TransactionId)
                {
                    throw new ArgumentException($"Transaction for context {transaction.Key} not current");
                }

                await context.SaveChangesAsync();
                await context.CurrentTransaction!.CommitAsync();
                logger.LogInformation("Transaction commit for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            _transaction.SetStatus(TransactionStatusEnum.Committed);
            currentInfo = _transaction;
        }
        catch (Exception e)
        {
            logger.LogError("Error while commit transaction: {Message}", e.Message);
            foreach (var transaction in _transaction.Context)
            {
                await transaction.Value.RollbackAsync();
                logger.LogInformation("Transaction rollback for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            _transaction.SetMessage(e.Message);
            _transaction.SetStatus(TransactionStatusEnum.Failed);
            currentInfo = _transaction;
        }
        finally
        {
            foreach (var transaction in _transaction.Context)
            {
                await transaction.Value.DisposeAsync();
            }

            _transaction = null;

            logger.LogInformation("Transactions end for user {UserId}", userId);
        }

        return currentInfo;
    }

    public async Task<TransactionInfo> RollbackTransactionAsync(Guid userId, Exception e)
    {
        if (_transaction is null)
        {
            throw new ArgumentException($"Transaction for user {userId} not found or not started");
        }

        TransactionInfo currentInfo;
        try
        {
            foreach (var transaction in _transaction.Context)
            {
                if (transaction.Value.TransactionId != context.CurrentTransaction?.TransactionId)
                {
                    throw new ArgumentException($"Transaction for context {transaction.Key} not current");
                }

                context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                await context.CurrentTransaction.RollbackAsync();
                logger.LogInformation("Transaction rollback for context {ContextName}, {TransactionId}",
                    transaction.Key, transaction.Value.TransactionId);
            }

            _transaction.SetMessage(e.Message);
            _transaction.SetStatus(TransactionStatusEnum.RolledBack);
            currentInfo = _transaction;
        }
        catch (Exception exception)
        {
            logger.LogError("Error while rollback transaction: {Message}", exception.Message);
            _transaction.SetMessage(exception.Message);
            _transaction.SetStatus(TransactionStatusEnum.Failed);
            currentInfo = _transaction;
        }
        finally
        {
            foreach (var transaction in _transaction.Context)
            {
                await transaction.Value.DisposeAsync();
            }

            _transaction = null;

            logger.LogInformation("Transactions end for user {UserId}", userId);
        }

        return currentInfo;
    }

    #region NotImplemented

    public Task BeginTransactionAsync(Guid userId, params Type[] contextName)
    {
        throw new NotImplementedException("Use for multi context transaction, use BeginTransactionAsync(Guid userId)");
    }

    public Task<TransactionInfo> CommitTransactionAsync(Guid userId, params Type[] contextName)
    {
        throw new NotImplementedException("Use for multi context transaction, use CommitTransactionAsync(Guid userId)");
    }

    public Task<TransactionInfo> RollbackTransactionAsync(Guid userId, Exception e, params Type[] contextName)
    {
        throw new NotImplementedException(
            "Use for multi context transaction, use RollbackTransactionAsync(Guid userId)");
    }

    #endregion
}