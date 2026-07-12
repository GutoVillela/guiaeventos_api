using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Posts.Input;
using Presentation.Posts.Output;
using Repository.Persistence;

namespace Presentation.Posts;

public class PostModule : BaseModule
{
    const string BasePath = "/api/posts";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Posts");
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization();
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization();
        group.MapPost("/{id:int}/publish", PublishAsync).RequireAuthorization("AdminOnly");
        group.MapDelete("/{id:int}/publish", UnpublishAsync).RequireAuthorization("AdminOnly");
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization();
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        bool publishedOnly = false,
        int? authorId = null,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken ct = default)
    {
        var query = db.Posts
            .Where(x => !x.IsDeleted)
            .Include(x => x.Author)
            .AsQueryable();

        if (publishedOnly)
            query = query.Where(x => x.PublishedAt != null);

        if (authorId.HasValue)
            query = query.Where(x => x.AuthorId == authorId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Title.Contains(search));

        var ascending = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLower() switch
        {
            "title" => ascending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
            "date"  => ascending ? query.OrderBy(x => x.PublishedAt ?? x.CreatedAt) : query.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt),
            _       => query.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt),
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(PostResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var post = await db.Posts
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (post is null)
            return Results.NotFound();

        return Results.Ok(PostResponse.FromEntity(post));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreatePostRequest request,
        CancellationToken ct)
    {
        var slugExists = await db.Posts.AnyAsync(x => x.Slug == request.Slug, ct);
        if (slugExists)
            return Results.Conflict("A post with this slug already exists.");

        var authorExists = await db.Authors.AnyAsync(x => x.Id == request.AuthorId, ct);
        if (!authorExists)
            return Results.NotFound("Author not found.");

        var post = new Post(request.Title, request.Slug, request.Summary, request.Content, request.AuthorId)
        {
            CreatedBy = "system"
        };

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
            post.SetCoverImage(Image.Create(request.CoverImageUrl, request.CoverImageAltText));

        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{post.Id}", PostResponse.FromEntity(post));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdatePostRequest request,
        CancellationToken ct)
    {
        var post = await db.Posts.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post is null)
            return Results.NotFound();

        var slugConflict = await db.Posts.AnyAsync(x => x.Slug == request.Slug && x.Id != id, ct);
        if (slugConflict)
            return Results.Conflict("Another post with this slug already exists.");

        post.Update(request.Title, request.Slug, request.Summary, request.Content);

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
            post.SetCoverImage(Image.Create(request.CoverImageUrl, request.CoverImageAltText));

        await db.SaveChangesAsync(ct);

        return Results.Ok(PostResponse.FromEntity(post));
    }

    private async Task<IResult> PublishAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post is null)
            return Results.NotFound();

        if (post.IsPublished)
            return Results.Conflict("Post is already published.");

        post.Publish();
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { post.Id, post.PublishedAt });
    }

    private async Task<IResult> UnpublishAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post is null)
            return Results.NotFound();

        if (!post.IsPublished)
            return Results.Conflict("Post is not published.");

        post.Unpublish();
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (post is null)
            return Results.NotFound();

        post.IsDeleted = true;
        post.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
