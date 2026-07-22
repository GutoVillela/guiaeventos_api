using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Notifications.Input;
using Presentation.Notifications.Output;
using Repository.Persistence;

namespace Presentation.Notifications;

public class NotificationModule : BaseModule
{
    const string BasePath = "/api/notifications";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Notifications").RequireAuthorization("AdminOnly");
        group.MapGet("/", ListAsync);
        group.MapGet("/unread-count", UnreadCountAsync);
        group.MapPost("/ack", AckAsync);
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 30,
        CancellationToken ct = default)
    {
        var query = db.Notifications
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(NotificationResponse.FromEntity) });
    }

    private async Task<IResult> UnreadCountAsync(
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var count = await db.Notifications
            .Where(x => !x.IsDeleted && x.AckedAt == null)
            .CountAsync(ct);

        return Results.Ok(new { count });
    }

    private async Task<IResult> AckAsync(
        [FromServices] AppDbContext db,
        [FromBody] AckNotificationsRequest request,
        CancellationToken ct)
    {
        var query = db.Notifications
            .Where(x => !x.IsDeleted && x.AckedAt == null);

        if (request.UpToId.HasValue)
            query = query.Where(x => x.Id <= request.UpToId.Value);

        var notifications = await query.ToListAsync(ct);

        foreach (var notification in notifications)
            notification.Ack();

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { ackedCount = notifications.Count });
    }
}
