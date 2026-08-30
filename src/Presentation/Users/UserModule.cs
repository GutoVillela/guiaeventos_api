using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Carter;
using Domain.Entities;
using Domain.Enums;
using Shared.Email;
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
        group.MapPost("/forgot-password", ForgotPasswordAsync);
        group.MapPost("/reset-password", ResetPasswordAsync);
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

    private async Task<IResult> ForgotPasswordAsync(
        [FromServices] AppDbContext db,
        [FromServices] IEmailService emailService,
        [FromServices] IConfiguration config,
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null)
            return Results.NotFound("Email not found.");

        // Invalidate any previous unused tokens for this user
        var previousTokens = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync(ct);
        foreach (var old in previousTokens)
            old.MarkAsUsed();

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var resetToken = new PasswordResetToken(user.Id, otp, expirationMinutes: 30);
        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync(ct);

        var frontendUrl = config["App:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var resetLink = $"{frontendUrl}/redefinir-senha?token={otp}";

        var subject = "Redefinição de senha — Guia Evento Escolar";
        var body = BuildPasswordResetEmail(user.Name, otp, resetLink);
        await emailService.SendAsync(user.Email, user.Name, subject, body, ct);

        return Results.Ok("Password reset email sent.");
    }

    private async Task<IResult> ResetPasswordAsync(
        [FromServices] AppDbContext db,
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var resetToken = await db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

        if (resetToken is null)
            return Results.NotFound("Token not found.");

        if (!resetToken.IsValid())
            return Results.BadRequest(resetToken.IsUsed ? "Token already used." : "Token expired.");

        resetToken.User.ChangePassword(request.NewPassword);
        resetToken.MarkAsUsed();
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static string BuildPasswordResetEmail(string name, string otp, string resetLink) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <body style="margin:0;padding:0;background:#f3f4f6;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:40px 16px;">
            <tr><td align="center">
              <table width="100%" style="max-width:520px;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;">
                <tr>
                  <td style="background:linear-gradient(135deg,#7c3aed,#a855f7);padding:32px 40px;text-align:center;">
                    <h1 style="margin:0;color:#ffffff;font-size:22px;font-weight:700;">Guia Evento Escolar</h1>
                    <p style="margin:8px 0 0;color:rgba(255,255,255,0.8);font-size:14px;">Redefinição de senha</p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:36px 40px;">
                    <p style="margin:0 0 16px;color:#374151;font-size:15px;">Olá, <strong>{name}</strong>!</p>
                    <p style="margin:0 0 24px;color:#6b7280;font-size:14px;line-height:1.6;">
                      Recebemos uma solicitação para redefinir a senha da sua conta.
                      Use o código abaixo ou clique no botão para criar uma nova senha.
                    </p>
                    <div style="background:#f9fafb;border:1px solid #e5e7eb;border-radius:8px;padding:20px;text-align:center;margin-bottom:24px;">
                      <p style="margin:0 0 6px;color:#6b7280;font-size:12px;text-transform:uppercase;letter-spacing:1px;">Seu código</p>
                      <p style="margin:0;font-size:36px;font-weight:700;color:#7c3aed;letter-spacing:8px;">{otp}</p>
                    </div>
                    <p style="margin:0 0 8px;color:#6b7280;font-size:13px;text-align:center;">ou</p>
                    <div style="text-align:center;margin:16px 0 24px;">
                      <a href="{resetLink}" style="display:inline-block;background:#7c3aed;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 28px;border-radius:8px;">
                        Redefinir minha senha
                      </a>
                    </div>
                    <p style="margin:0;color:#9ca3af;font-size:12px;line-height:1.6;">
                      Este código expira em <strong>30 minutos</strong>.<br/>
                      Se você não solicitou a redefinição de senha, ignore este e-mail — sua senha não será alterada.
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="background:#f9fafb;padding:16px 40px;text-align:center;border-top:1px solid #e5e7eb;">
                    <p style="margin:0;color:#9ca3af;font-size:12px;">© Guia Evento Escolar — Todos os direitos reservados</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

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
