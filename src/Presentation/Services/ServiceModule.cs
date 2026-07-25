using System.Security.Claims;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.FileStorage;
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
        group.MapPost("/", CreateAsync).RequireAuthorization().DisableAntiforgery();
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapPut("/{id:int}/approve", ApproveAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}/reject", RejectAsync).RequireAuthorization("AdminOnly");
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken ct = default)
    {
        var query = db.Services
            .Where(x => !x.IsDeleted)
            .Include(x => x.Categories)
            .Include(x => x.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EAdvertisementStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);

        var ascending = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLower() switch
        {
            "name"   => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
            "status" => ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
            _        => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
        };

        var total = await query.CountAsync(ct);
        var items = await query
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
            .Where(x => !x.IsDeleted)
            .Include(x => x.Categories)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (service is null)
            return Results.NotFound();

        return Results.Ok(ServiceResponse.FromEntity(service));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromServices] IFileStorageService fileStorage,
        [FromForm] int advertiserId,
        [FromForm] string name,
        [FromForm] string description,
        [FromForm] string? summary,
        [FromForm] int[] categoryIds,
        [FromForm] string phoneAreaCode,
        [FromForm] string phoneNumber,
        [FromForm] int mainImageIndex,
        IFormFileCollection images,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phoneAreaCode) || string.IsNullOrWhiteSpace(phoneNumber))
            return Results.BadRequest("O telefone (WhatsApp) é obrigatório.");

        if (images == null || images.Count == 0)
            return Results.BadRequest("Pelo menos uma imagem deve ser enviada.");

        if (mainImageIndex < 0 || mainImageIndex >= images.Count)
            return Results.BadRequest("Índice da imagem principal inválido.");

        if (categoryIds == null || categoryIds.Length == 0)
            return Results.BadRequest("Pelo menos uma categoria deve ser informada.");

        var advertiser = await db.Users.FindAsync([advertiserId], ct);
        if (advertiser is null)
            return Results.BadRequest("Advertiser not found.");

        var categories = await db.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(ct);

        if (categories.Count != categoryIds.Length)
            return Results.BadRequest("Uma ou mais categorias informadas não foram encontradas.");

        var service = new Service(name, description, summary ?? string.Empty, advertiser)
        {
            CreatedBy = "system"
        };
        service.SetCategories(categories);
        service.SetPhone(Phone.Create(phoneAreaCode, phoneNumber));

        var orderedFiles = images.ToList();
        var mainFile = orderedFiles[mainImageIndex];
        orderedFiles.RemoveAt(mainImageIndex);
        orderedFiles.Insert(0, mainFile);

        var imageList = new List<Image>();
        foreach (var file in orderedFiles)
        {
            var url = await fileStorage.UploadAsync(file, ct);
            imageList.Add(Image.Create(url, null));
        }
        service.SetImages(imageList);

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
        var service = await db.Services
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (service is null)
            return Results.NotFound();

        service.Update(request.Name, request.Description, request.Summary ?? string.Empty);

        if (request.CategoryIds is { Length: > 0 })
        {
            var categories = await db.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync(ct);
            service.SetCategories(categories);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneAreaCode) && !string.IsNullOrWhiteSpace(request.PhoneNumber))
            service.SetPhone(Phone.Create(request.PhoneAreaCode, request.PhoneNumber));

        service.ResetToPendingApproval();
        await db.SaveChangesAsync(ct);

        return Results.Ok(ServiceResponse.FromEntity(service));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var service = await db.Services
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (service is null)
            return Results.NotFound();

        service.IsDeleted = true;
        service.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private async Task<IResult> ApproveAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var service = await db.Services
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (service is null)
            return Results.NotFound();

        var approvedBy = user.FindFirstValue("unique_name") ?? user.FindFirstValue("sub") ?? "system";
        service.Approve(approvedBy);
        await db.SaveChangesAsync(ct);

        return Results.Ok(ServiceResponse.FromEntity(service));
    }

    private async Task<IResult> RejectAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] RejectRequest request,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var service = await db.Services
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (service is null)
            return Results.NotFound();

        var rejectedBy = user.FindFirstValue("unique_name") ?? user.FindFirstValue("sub") ?? "system";
        service.Reject(rejectedBy, request.Reason);
        await db.SaveChangesAsync(ct);

        return Results.Ok(ServiceResponse.FromEntity(service));
    }
}
