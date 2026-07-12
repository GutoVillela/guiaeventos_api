using Quartz;
using Shared.Outbox;

namespace Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class OutboxMessagesProcessorJob : IJob
{
    private readonly IOutboxProcessor _outboxProcessor;

    public OutboxMessagesProcessorJob(IOutboxProcessor outboxProcessor)
    {
        _outboxProcessor = outboxProcessor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (_outboxProcessor is null) throw new ArgumentNullException(nameof(_outboxProcessor), "Outbox processor was not loaded");
        await _outboxProcessor.ProcessOutboxMessagesAsync(context.CancellationToken);
    }
}
