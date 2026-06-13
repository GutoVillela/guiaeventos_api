using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class AdvertisementMap : EntityMap<Advertisement>
{
    public override void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        base.Configure(builder);

        builder.ToTable("advertisements");

        builder.HasDiscriminator<string>("Type")
            .HasValue<Place>("Place")
            .HasValue<Service>("Service");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.Website).HasMaxLength(300).IsRequired(false);
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>(
                v => v.ToString(),
                v => (EAdvertisementStatus)Enum.Parse(typeof(EAdvertisementStatus), v))
            .HasMaxLength(50)
            .HasDefaultValue(EAdvertisementStatus.PendingApproval);

        builder.HasIndex(x => x.Name);

        builder.OwnsOne(x => x.Phone, phone =>
        {
            phone.Property(p => p.AreaCode).HasColumnName("Phone_AreaCode").HasMaxLength(5);
            phone.Property(p => p.Number).HasColumnName("Phone_Number").HasMaxLength(15);
        });

        builder.OwnsMany(x => x.Images, imageBuilder =>
        {
            imageBuilder.ToTable("advertisement_images");
            imageBuilder.WithOwner().HasForeignKey("AdvertisementId");
            imageBuilder.Property<int>("Id").ValueGeneratedOnAdd();
            imageBuilder.HasKey("Id");
            imageBuilder.Property(x => x.Url).HasMaxLength(500).IsRequired();
            imageBuilder.Property(x => x.AltText).HasMaxLength(300).IsRequired(false);
        });
    }

    protected override void ConfigureRelationships(EntityTypeBuilder<Advertisement> builder)
    {
        base.ConfigureRelationships(builder);

        builder.HasOne(x => x.Advertiser)
            .WithMany(u => u.Advertisements)
            .HasForeignKey("AdvertiserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Categories)
            .WithMany(c => c.Advertisements)
            .UsingEntity(j => j.ToTable("advertisement_categories"));
    }
}
