using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Banners.Output;
using Presentation.FileStorage;
using Repository.Persistence;

namespace Presentation.Banners;

public class BannerModule : BaseModule
{
    const string BasePath = "/api/banners";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Banners");

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization().DisableAntiforgery();
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization().DisableAntiforgery();
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization();
        group.MapPut("/{id:int}/activate", ActivateAsync).RequireAuthorization();
        group.MapPut("/{id:int}/deactivate", DeactivateAsync).RequireAuthorization();
    }

    private static async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var total = await db.Banners.CountAsync(ct);
        var items = await db.Banners
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(BannerResponse.FromEntity) });
    }

    private static async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (banner is null)
            return Results.NotFound();

        return Results.Ok(BannerResponse.FromEntity(banner));
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromServices] IFileStorageService storage,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string linkUrl,
        [FromForm] int order,
        [FromForm] DateTimeOffset? startsAt,
        [FromForm] DateTimeOffset? endsAt,
        IFormFile image,
        [FromForm] string? imageAltText,
        CancellationToken ct)
    {
        var imageUrl = await storage.UploadAsync(image, ct);

        var banner = new Banner(title, linkUrl, Image.Create(imageUrl, imageAltText), order, description, startsAt, endsAt)
        {
            CreatedBy = "system"
        };

        db.Banners.Add(banner);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{banner.Id}", BannerResponse.FromEntity(banner));
    }

    private static async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromServices] IFileStorageService storage,
        [FromRoute] int id,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string linkUrl,
        [FromForm] int order,
        [FromForm] DateTimeOffset? startsAt,
        [FromForm] DateTimeOffset? endsAt,
        IFormFile? image,
        [FromForm] string? imageAltText,
        CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (banner is null)
            return Results.NotFound();

        Image updatedImage;
        if (image is not null)
        {
            var imageUrl = await storage.UploadAsync(image, ct);
            updatedImage = Image.Create(imageUrl, imageAltText);
        }
        else
        {
            updatedImage = Image.Create(banner.Image.Url, imageAltText ?? banner.Image.AltText);
        }

        banner.Update(title, linkUrl, updatedImage, order, description, startsAt, endsAt);
        await db.SaveChangesAsync(ct);

        return Results.Ok(BannerResponse.FromEntity(banner));
    }

    private static async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (banner is null)
            return Results.NotFound();

        banner.IsDeleted = true;
        banner.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<IResult> ActivateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (banner is null)
            return Results.NotFound();

        banner.Activate();
        await db.SaveChangesAsync(ct);

        return Results.Ok(BannerResponse.FromEntity(banner));
    }

    private static async Task<IResult> DeactivateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (banner is null)
            return Results.NotFound();

        banner.Deactivate();
        await db.SaveChangesAsync(ct);

        return Results.Ok(BannerResponse.FromEntity(banner));
    }
}
