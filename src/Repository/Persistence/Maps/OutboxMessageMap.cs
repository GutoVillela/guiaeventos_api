using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Repository.Outbox;

namespace Repository.Persistence.Maps;

internal class OutboxMessageMap : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>(
            enumValue => enumValue.ToString(),
            stringValue => (EOutboxMessageStatus)Enum.Parse(typeof(EOutboxMessageStatus), stringValue)
            );
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.Error);
        builder.Ignore(x => x.IsProcessed);
    }
}
