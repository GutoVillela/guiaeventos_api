using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class BannerMap : EntityMap<Banner>
{
    public override void Configure(EntityTypeBuilder<Banner> builder)
    {
        base.Configure(builder);

        builder.ToTable("banners");

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.LinkUrl).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Order).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.StartsAt).IsRequired(false);
        builder.Property(x => x.EndsAt).IsRequired(false);

        builder.HasIndex(x => x.Order);
        builder.HasIndex(x => x.IsActive);

        builder.OwnsOne(x => x.Image, image =>
        {
            image.Property(i => i.Url).HasColumnName("Image_Url").HasMaxLength(500).IsRequired();
            image.Property(i => i.AltText).HasColumnName("Image_AltText").HasMaxLength(300).IsRequired(false);
        });
    }
}
