using Domain.Entities;

namespace Presentation.Notifications.Output;

public record NotificationResponse(
    int Id,
    string Title,
    string Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AckedAt)
{
    public static NotificationResponse FromEntity(Notification n) =>
        new(n.Id, n.Title, n.Message, n.CreatedAt, n.AckedAt);
}
