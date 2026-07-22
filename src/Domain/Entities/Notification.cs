using Domain.Primitives;

namespace Domain.Entities;

public class Notification : Entity
{
    public Notification(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public DateTimeOffset? AckedAt { get; private set; }
    public string Title { get; init; }
    public string Message { get; set; }

    public void Ack()
    {
        AckedAt = DateTimeOffset.Now;
    }
}
