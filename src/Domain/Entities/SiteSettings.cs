using Domain.Primitives;

namespace Domain.Entities;

public class SiteSettings : Entity
{
    public bool IsMaintenanceMode { get; private set; } = false;

    protected SiteSettings() { }

    public SiteSettings(string createdBy)
    {
        CreatedBy = createdBy;
        IsMaintenanceMode = false;
    }

    public void Update(bool isMaintenanceMode)
    {
        IsMaintenanceMode = isMaintenanceMode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
