using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class AuthorMap : EntityMap<Author>
{
    public override void Configure(EntityTypeBuilder<Author> builder)
    {
        base.Configure(builder);

        builder.ToTable("authors");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
    }

    protected override void ConfigureRelationships(EntityTypeBuilder<Author> builder)
    {
        base.ConfigureRelationships(builder);

        builder.HasMany(x => x.Posts)
            .WithOne()
            .HasForeignKey("AuthorId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
