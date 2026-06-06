using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Persistence.Maps;

internal class BannerItemMap : EntityMap<BannerItem>
{
    public override void Configure(EntityTypeBuilder<BannerItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("banner_items");
    }
}
