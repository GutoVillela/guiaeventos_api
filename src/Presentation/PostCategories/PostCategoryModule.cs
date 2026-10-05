using System.Globalization;
using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.PostCategories.Input;
using Presentation.PostCategories.Output;
using Repository.Persistence;

namespace Presentation.PostCategories;

public class PostCategoryModule : BaseModule
{
    const string BasePath = "/api/post-categories";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("PostCategories");
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}/activate", ActivateAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}/deactivate", DeactivateAsync).RequireAuthorization("AdminOnly");
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("AdminOnly");
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 100,
        bool activeOnly = false,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken ct = default)
    {
        var query = db.PostCategories
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search));

        var ascending = !string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            _ => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(PostCategoryResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var category = await db.PostCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        return Results.Ok(PostCategoryResponse.FromEntity(category));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreatePostCategoryRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        var name = request.Name.Trim();
        var exists = await db.PostCategories.AnyAsync(x => x.Name == name, ct);
        if (exists)
            return Results.Conflict("A post category with this name already exists.");

        var slug = await GenerateUniqueSlugAsync(db, name, null, ct);
        var category = new PostCategory(name, slug, request.Color) { CreatedBy = "system" };

        db.PostCategories.Add(category);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{category.Id}", PostCategoryResponse.FromEntity(category));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdatePostCategoryRequest request,
        CancellationToken ct)
    {
        var category = await db.PostCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        var name = request.Name.Trim();
        var nameConflict = await db.PostCategories.AnyAsync(x => x.Name == name && x.Id != id, ct);
        if (nameConflict)
            return Results.Conflict("Another post category with this name already exists.");

        var slug = await GenerateUniqueSlugAsync(db, name, id, ct);
        category.Update(name, slug, request.Color);
        await db.SaveChangesAsync(ct);

        return Results.Ok(PostCategoryResponse.FromEntity(category));
    }

    private async Task<IResult> ActivateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var category = await db.PostCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        category.Activate();
        await db.SaveChangesAsync(ct);

        return Results.Ok(PostCategoryResponse.FromEntity(category));
    }

    private async Task<IResult> DeactivateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var category = await db.PostCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        category.Deactivate();
        await db.SaveChangesAsync(ct);

        return Results.Ok(PostCategoryResponse.FromEntity(category));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var category = await db.PostCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        category.IsDeleted = true;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<string> GenerateUniqueSlugAsync(AppDbContext db, string name, int? ignoreId, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "categoria";

        var slug = baseSlug;
        var suffix = 2;
        while (await db.PostCategories.AnyAsync(x => x.Slug == slug && (ignoreId == null || x.Id != ignoreId), ct))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
                builder.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_')
                builder.Append('-');
        }

        var slug = builder.ToString().Normalize(NormalizationForm.FormC);
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        return slug.Trim('-');
    }
}
