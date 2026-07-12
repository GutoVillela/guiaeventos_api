using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Carter;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Presentation.Users.Input;
using Presentation.Users.Output;
using Repository.Persistence;

namespace Presentation.Users;

public class UserModule : BaseModule
{
    const string BasePath = "/api/users";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Users");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/", CreateAsync);
        group.MapPost("/admin", CreateAdminAsync).RequireAuthorization("AdminOnly");
        group.MapPost("/first", CreateFirstUserAsync);
        group.MapGet("/", ListAsync).RequireAuthorization();
        group.MapGet("/{id:int}", GetByIdAsync).RequireAuthorization();
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization();
        group.MapPut("/{id:int}/password", ChangePasswordAsync).RequireAuthorization();
        group.MapDelete("/{id:int}", DeactivateAsync).RequireAuthorization();
        group.MapPut("/{id:int}/activate", ReactivateAsync).RequireAuthorization();
    }

    private async Task<IResult> LoginAsync(
        [FromServices] AppDbContext db,
        [FromServices] IConfiguration config,
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Username == request.Username || u.Email == request.Email, ct);

        if (user is null || !user.Password.Verify(request.Password))
            return Results.Unauthorized();

        if (!user.IsActive)
            return Results.Problem("User account is deactivated.", statusCode: StatusCodes.Status403Forbidden);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var token = GenerateJwt(user, config, expiresAt);

        return Results.Ok(new LoginResponse(token, expiresAt, UserResponse.FromEntity(user)));
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var usernameExists = await db.Users.AnyAsync(u => u.Username == request.Username, ct);
        if (usernameExists)
            return Results.Conflict("Username already taken.");

        var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (emailExists)
            return Results.Conflict("Email already registered.");

        var user = new User(request.Name, request.Username, request.Email, request.Password)
        {
            CreatedBy = "system"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{user.Id}", UserResponse.FromEntity(user));
    }

    private async Task<IResult> CreateAdminAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var usernameExists = await db.Users.AnyAsync(u => u.Username == request.Username, ct);
        if (usernameExists)
            return Results.Conflict("Username already taken.");

        var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (emailExists)
            return Results.Conflict("Email already registered.");

        var user = new User(request.Name, request.Username, request.Email, request.Password)
        {
            CreatedBy = "system"
        };
        user.SetRole(EUserRole.Admin);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{user.Id}", UserResponse.FromEntity(user));
    }

    private async Task<IResult> CreateFirstUserAsync(
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var anyUsers = await db.Users.AnyAsync(ct);
        if (anyUsers)
            return Results.Conflict("Users already exist. This endpoint is only for creating the first user.");
        var user = new User(
            name: "admin",
            username: "admin",
            email: "admin@example.com",
            rawPassword: "AdminPassword123!"
        )
        {
            CreatedBy = "system"
        };
        user.SetRole(EUserRole.Admin);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return Results.Created($"{BasePath}/{user.Id}", UserResponse.FromEntity(user));
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
        var query = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

        var ascending = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
            _      => ascending ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(UserResponse.FromEntity) });
    }

    private async Task<IResult> GetByIdAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound();

        return Results.Ok(UserResponse.FromEntity(user));
    }

    private async Task<IResult> UpdateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound();

        var emailConflict = await db.Users.AnyAsync(u => u.Email == request.Email && u.Id != id, ct);
        if (emailConflict)
            return Results.Conflict("Email already in use by another user.");

        user.Update(request.Name, request.Email);
        await db.SaveChangesAsync(ct);

        return Results.Ok(UserResponse.FromEntity(user));
    }

    private async Task<IResult> ChangePasswordAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound();

        if (!user.Password.Verify(request.CurrentPassword))
            return Results.BadRequest("Current password is incorrect.");

        user.ChangePassword(request.NewPassword);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private async Task<IResult> DeactivateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound();

        if (!user.IsActive)
            return Results.Conflict("User is already deactivated.");

        user.Deactivate();
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private async Task<IResult> ReactivateAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound();

        if (user.IsActive)
            return Results.Conflict("User is already active.");

        user.Reactivate();
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static string GenerateJwt(User user, IConfiguration config, DateTimeOffset expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("unique_name", user.Username),
            new Claim("email", user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
