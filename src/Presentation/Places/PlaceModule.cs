using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Places.Input;
using Presentation.Places.Output;
using Repository.Persistence;

namespace Presentation.Places;

public class PlaceModule : BaseModule
{
    const string BasePath = "/api/places";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Places");
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var total = await db.Places.CountAsync(ct);
        var items = await db.Places
            .Include(x => x.Categories)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(PlaceResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var place = await db.Places
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (place is null)
            return Results.NotFound();

        return Results.Ok(PlaceResponse.FromEntity(place));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreatePlaceRequest request,
        CancellationToken ct)
    {
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

        var location = Address.Create(
            request.Street,
            request.Neighborhood ?? string.Empty,
            request.City,
            request.State,
            request.Country,
            request.ZipCode ?? string.Empty,
            request.Number ?? string.Empty,
            request.Complement ?? string.Empty,
            request.ReferencePoint ?? string.Empty);

        var place = new Place(request.Name, request.Description, request.Summary ?? string.Empty, advertiser, location)
        {
            CreatedBy = "system"
        };
        place.SetCategories(categories);

        db.Places.Add(place);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{place.Id}", PlaceResponse.FromEntity(place));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdatePlaceRequest request,
        CancellationToken ct)
    {
        var place = await db.Places.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (place is null)
            return Results.NotFound();

        var location = Address.Create(
            request.Street,
            request.Neighborhood ?? string.Empty,
            request.City,
            request.State,
            request.Country,
            request.ZipCode ?? string.Empty,
            request.Number ?? string.Empty,
            request.Complement ?? string.Empty,
            request.ReferencePoint ?? string.Empty);

        place.Update(request.Name, request.Description, request.Summary ?? string.Empty, location);
        await db.SaveChangesAsync(ct);

        return Results.Ok(PlaceResponse.FromEntity(place));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var place = await db.Places.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (place is null)
            return Results.NotFound();

        place.IsDeleted = true;
        place.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
