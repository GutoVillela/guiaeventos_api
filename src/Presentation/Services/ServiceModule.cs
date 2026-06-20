using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Services.Input;
using Presentation.Services.Output;
using Repository.Persistence;

namespace Presentation.Services;

public class ServiceModule : BaseModule
{
    const string BasePath = "/api/services";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Services");
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization();
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var total = await db.Services.CountAsync(ct);
        var items = await db.Services
            .Include(x => x.Categories)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(ServiceResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var service = await db.Services
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (service is null)
            return Results.NotFound();

        return Results.Ok(ServiceResponse.FromEntity(service));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreateServiceRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneAreaCode) || string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Results.BadRequest("O telefone (WhatsApp) é obrigatório.");

        var categoryIds = request.CategoryIds?.ToList() ?? [];
        if (categoryIds.Count == 0)
            return Results.BadRequest("Pelo menos uma categoria deve ser informada.");

        var advertiser = await db.Users.FindAsync([request.AdvertiserId], ct);
        if (advertiser is null)
            return Results.BadRequest("Advertiser not found.");

        var categories = await db.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(ct);

        if (categories.Count != categoryIds.Count)
            return Results.BadRequest("Uma ou mais categorias informadas não foram encontradas.");

        var service = new Service(request.Name, request.Description, request.Summary ?? string.Empty, advertiser)
        {
            CreatedBy = "system"
        };
        service.SetCategories(categories);
        service.SetPhone(Phone.Create(request.PhoneAreaCode, request.PhoneNumber));

        db.Services.Add(service);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{service.Id}", ServiceResponse.FromEntity(service));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken ct)
    {
        var service = await db.Services.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (service is null)
            return Results.NotFound();

        service.Update(request.Name, request.Description, request.Summary ?? string.Empty);
        await db.SaveChangesAsync(ct);

        return Results.Ok(ServiceResponse.FromEntity(service));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var service = await db.Services.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (service is null)
            return Results.NotFound();

        service.IsDeleted = true;
        service.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
