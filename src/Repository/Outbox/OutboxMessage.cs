namespace Repository.Outbox;

internal class OutboxMessage
{
    public OutboxMessage(
        Guid id, 
        string type, 
        string payload)
    {
        Id = id;
        Type = type;
        Payload = payload;
    }

    
    public Guid Id { get; init; }
    public string Type { get; init; }
    public string Payload { get; init; }
    public EOutboxMessageStatus Status { get; private set; } = EOutboxMessageStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }
    public bool IsProcessed => ProcessedAt.HasValue;

    internal void FailOrRetry(string errorMessage, int maxRetries = 3)
    {
        ProcessedAt = DateTimeOffset.Now;
        Error = errorMessage;
        RetryCount++;

        if(RetryCount >= maxRetries)
            Status = EOutboxMessageStatus.Failed;
        else
            Status = EOutboxMessageStatus.Retrying;
    }

    internal void MaskAsProcessed()
    {
        ProcessedAt = DateTimeOffset.Now;
        Status = EOutboxMessageStatus.Processed;
    }
}

public enum EOutboxMessageStatus
{
    Pending,
    Processed,
    Retrying,
    Failed
}
