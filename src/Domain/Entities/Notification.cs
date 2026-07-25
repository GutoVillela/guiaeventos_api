using Domain.Enums;
using Domain.Primitives;

namespace Domain.Entities;

public class Notification : Entity
{
    public Notification(string title, string message, int? referenceId = null, ENotificationReferenceType? referenceType = null)
    {
        Title = title;
        Message = message;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
    }

    public DateTimeOffset? AckedAt { get; private set; }
    public string Title { get; init; }
    public string Message { get; set; }
    public int? ReferenceId { get; init; }
    public ENotificationReferenceType? ReferenceType { get; init; }

    public void Ack()
    {
        AckedAt = DateTimeOffset.Now;
    }
}
