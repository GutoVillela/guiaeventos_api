using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Categories.Input;
using Presentation.Categories.Output;
using Repository.Persistence;

namespace Presentation.Categories;

public class CategoryModule : BaseModule
{
    const string BasePath = "/api/categories";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Categories");
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization("AdminOnly");
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("AdminOnly");
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken ct = default)
    {
        var query = db.Categories
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search));

        var ascending = !string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            _      => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(CategoryResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        return Results.Ok(CategoryResponse.FromEntity(category));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct)
    {
        var exists = await db.Categories.AnyAsync(x => x.Name == request.Name, ct);
        if (exists)
            return Results.Conflict("A category with this name already exists.");

        var category = new Category(request.Name, request.Description ?? string.Empty)
        {
            CreatedBy = "system"
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{category.Id}", CategoryResponse.FromEntity(category));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        var nameConflict = await db.Categories.AnyAsync(x => x.Name == request.Name && x.Id != id, ct);
        if (nameConflict)
            return Results.Conflict("Another category with this name already exists.");

        category.Update(request.Name, request.Description ?? string.Empty);
        await db.SaveChangesAsync(ct);

        return Results.Ok(CategoryResponse.FromEntity(category));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null)
            return Results.NotFound();

        category.IsDeleted = true;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
