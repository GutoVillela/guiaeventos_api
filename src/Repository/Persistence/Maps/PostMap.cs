using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class PostMap : EntityMap<Post>
{
    public override void Configure(EntityTypeBuilder<Post> builder)
    {
        base.Configure(builder);

        builder.ToTable("posts");

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(350).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.PublishedAt).IsRequired(false);
        builder.Property(x => x.AuthorId).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.PublishedAt);

        builder.OwnsOne(x => x.CoverImage, image =>
        {
            image.Property(i => i.Url).HasColumnName("CoverImage_Url").HasMaxLength(500).IsRequired();
            image.Property(i => i.AltText).HasColumnName("CoverImage_AltText").HasMaxLength(300).IsRequired(false);
        });
    }

    protected override void ConfigureRelationships(EntityTypeBuilder<Post> builder)
    {
        base.ConfigureRelationships(builder);

        builder.HasOne(x => x.Author)
            .WithMany(a => a.Posts)
            .HasForeignKey(x => x.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
