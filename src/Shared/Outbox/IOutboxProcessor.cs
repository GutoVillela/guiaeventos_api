namespace Shared.Outbox;

public interface IOutboxProcessor
{
    Task ProcessOutboxMessagesAsync(
        CancellationToken cancellationToken = default,
        int batchSize = 1000,
        TimeSpan? delay = null
        );
}
