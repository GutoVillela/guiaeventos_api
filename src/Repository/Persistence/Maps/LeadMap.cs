using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class LeadMap : EntityMap<Lead>
{
    public override void Configure(EntityTypeBuilder<Lead> builder)
    {
        base.Configure(builder);

        builder.ToTable("leads");

        builder.Property(x => x.Source).HasMaxLength(30).IsRequired().HasDefaultValue("Advertisement");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.Company).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.AdvertisementId).IsRequired(false);
        builder.Property(x => x.AdvertisementType).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired(false);
        builder.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ReadAt).IsRequired(false);

        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.AdvertisementId);
        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => x.Email);
    }
}
