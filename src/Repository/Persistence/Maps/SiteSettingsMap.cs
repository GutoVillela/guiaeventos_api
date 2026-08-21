using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class SiteSettingsMap : EntityMap<SiteSettings>
{
    public override void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        base.Configure(builder);

        builder.ToTable("site_settings");

        builder.Property(x => x.IsMaintenanceMode).IsRequired().HasDefaultValue(false);
    }
}
