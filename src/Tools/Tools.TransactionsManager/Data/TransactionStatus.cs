namespace Tools.TransactionsManager.Data;

public class TransactionStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
}

internal static class TransactionStatusEnum
{
    internal static readonly TransactionStatus Started = new() { Status = "Started", StartedAt = DateTime.UtcNow };
    internal static readonly TransactionStatus Committed = new() { Status = "Committed", StartedAt = DateTime.UtcNow };

    internal static readonly TransactionStatus
        RolledBack = new() { Status = "RolledBack", StartedAt = DateTime.UtcNow };

    internal static readonly TransactionStatus Failed = new() { Status = "Failed", StartedAt = DateTime.UtcNow };
}