using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class EntityMap<T> : IEntityTypeConfiguration<T> where T : Entity
{
    const int CreatedByMaxLength = 50;

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        ConfigureProperties(builder);
    }

    protected void ConfigureProperties(EntityTypeBuilder<T> builder)
    {
        ConfigureEntityProperties(builder);
        ConfigureKeys(builder);
        ConfigureRelationships(builder);
        ConfigureForeignKeys(builder);
    }

    private void ConfigureEntityProperties(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(CreatedByMaxLength);
        builder.Property(x => x.IsDeleted).IsRequired();
    }

    protected virtual void ConfigureKeys(EntityTypeBuilder<T> builder)
    {
    }

    protected virtual void ConfigureRelationships(EntityTypeBuilder<T> builder)
    {
    }

    protected virtual void ConfigureForeignKeys(EntityTypeBuilder<T> builder)
    {
    }
}
