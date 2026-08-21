using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Repository.Persistence;

namespace Presentation.Settings;

public class SettingsModule : BaseModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");
        group.MapGet("/", GetAsync);
        group.MapPut("/", UpdateAsync).RequireAuthorization("AdminOnly");
    }

    private async Task<IResult> GetAsync(
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var settings = await db.SiteSettings.FirstOrDefaultAsync(ct);
        return Results.Ok(new { isMaintenanceMode = settings?.IsMaintenanceMode ?? false });
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromBody] UpdateSettingsRequest request,
        CancellationToken ct)
    {
        var settings = await db.SiteSettings.FirstOrDefaultAsync(ct);
        if (settings is null)
            return Results.NotFound("Configuracoes nao encontradas.");

        settings.Update(request.IsMaintenanceMode);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { settings.IsMaintenanceMode });
    }
}

public record UpdateSettingsRequest(bool IsMaintenanceMode);
