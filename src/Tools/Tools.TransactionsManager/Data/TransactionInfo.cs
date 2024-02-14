using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;

namespace Tools.TransactionsManager.Data;

public class TransactionInfo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public bool IsFailed { get; private set; } = false;
    public List<TransactionStatus> StatusHistory { get; init; } = [];
    public Dictionary<string, IDbContextTransaction> Context { get; init; } = new();
    public string Message { get; private set; } = string.Empty;

    public TransactionInfo(Guid userId)
    {
        UserId = userId;
        StartTime = DateTime.UtcNow;
        StatusHistory.Add(TransactionStatusEnum.Started);
    }

    public void AddTransaction(IDbContext context, IDbContextTransaction contextTransaction)
    {
        Context.Add(context.GetType().Name, contextTransaction);
    }

    public void HasFailed()
    {
        IsFailed = true;
        StatusHistory.Add(TransactionStatusEnum.Failed);
    }

    public void SetMessage(string message)
    {
        Message = message;

    }

    public void SetStatus(TransactionStatus status)
    {
        StatusHistory.Add(status);
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
}