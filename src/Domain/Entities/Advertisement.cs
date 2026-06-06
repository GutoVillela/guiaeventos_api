using Domain.Enums;

namespace Domain.Entities;

public abstract class Advertisement : Entity
{
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public EAdvertisementStatus Status { get; private set; } = EAdvertisementStatus.PendingApproval;
    public User Advertiser { get; private set; } = new User();

    public IEnumerable<Category> Categories { get; private set; } = new List<Category>();
}
