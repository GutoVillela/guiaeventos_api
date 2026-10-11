using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class PostCategoryMap : EntityMap<PostCategory>
{
    public override void Configure(EntityTypeBuilder<PostCategory> builder)
    {
        base.Configure(builder);

        builder.ToTable("post_categories");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsHighlighted).HasDefaultValue(false);
        builder.Property(x => x.HighlightOrder).HasDefaultValue(0);
        builder.Property(x => x.HighlightIcon).HasMaxLength(50).IsRequired(false);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsHighlighted);
    }
}
