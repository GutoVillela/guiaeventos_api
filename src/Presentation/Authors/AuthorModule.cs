using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Authors.Input;
using Presentation.Authors.Output;
using Repository.Persistence;

namespace Presentation.Authors;

public class AuthorModule : BaseModule
{
    const string BasePath = "/api/authors";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Authors");
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
        var query = db.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));

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

        return Results.Ok(new { total, page, pageSize, items = items.Select(AuthorResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var author = await db.Authors.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (author is null)
            return Results.NotFound();

        return Results.Ok(AuthorResponse.FromEntity(author));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreateAuthorRequest request,
        CancellationToken ct)
    {
        var exists = await db.Authors.AnyAsync(x => x.Email == request.Email, ct);
        if (exists)
            return Results.Conflict("An author with this email already exists.");

        var author = new Author(request.Name, request.Email, request.Bio) { CreatedBy = "system" };

        db.Authors.Add(author);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{author.Id}", AuthorResponse.FromEntity(author));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdateAuthorRequest request,
        CancellationToken ct)
    {
        var author = await db.Authors.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (author is null)
            return Results.NotFound();

        var emailConflict = await db.Authors.AnyAsync(x => x.Email == request.Email && x.Id != id, ct);
        if (emailConflict)
            return Results.Conflict("Another author with this email already exists.");

        author.Update(request.Name, request.Email, request.Bio);

        await db.SaveChangesAsync(ct);

        return Results.Ok(AuthorResponse.FromEntity(author));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var author = await db.Authors.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (author is null)
            return Results.NotFound();

        author.IsDeleted = true;
        author.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
