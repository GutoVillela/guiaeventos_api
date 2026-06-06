using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class PlaceMap : IEntityTypeConfiguration<Place>
{
    public void Configure(EntityTypeBuilder<Place> builder)
    {
        builder.OwnsOne(x => x.Location, address =>
        {
            address.Property(a => a.Street).HasColumnName("Location_Street").HasMaxLength(200).IsRequired();
            address.Property(a => a.Neighborhood).HasColumnName("Location_Neighborhood").HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("Location_City").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("Location_State").HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasColumnName("Location_Country").HasMaxLength(100).IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("Location_ZipCode").HasMaxLength(20);
            address.Property(a => a.Number).HasColumnName("Location_Number").HasMaxLength(20);
            address.Property(a => a.Complement).HasColumnName("Location_Complement").HasMaxLength(200);
            address.Property(a => a.ReferencePoint).HasColumnName("Location_ReferencePoint").HasMaxLength(300);
        });
    }
}
