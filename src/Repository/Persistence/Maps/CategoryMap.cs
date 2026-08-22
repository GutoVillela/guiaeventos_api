using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class CategoryMap : EntityMap<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("categories");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsHighlighted).HasDefaultValue(false);
        builder.Property(x => x.HighlightOrder).HasDefaultValue(0);
        builder.Property(x => x.HighlightColor).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.HighlightLink).HasMaxLength(500).IsRequired(false);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsHighlighted);
    }
}
