using System.Text;
using Carter;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Presentation;
using Repository;
using WebAPI.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();

string? connectionString = builder.Configuration.GetConnectionString("MySQL");
if (string.IsNullOrEmpty(connectionString))
    throw new EmptyConnectionStringException();

builder.Services
    .AddRepository(connectionString)
    .AddPresentation();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "Admin"));
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()));
}

// Tempor�rio, remover antes de ir para prod
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Repository.Persistence.AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao aplicar migrations. A aplicação continuará sem aplicar as migrations.");
    }

    try
    {
        var anyUsers = await db.Users.AnyAsync();
        if (!anyUsers)
        {
            var adminUser = new User(
                name: "Administrador",
                username: "admin",
                email: "admin@guiaeventos.com.br",
                rawPassword: "@dmin123"
            ) { CreatedBy = "system" };
            adminUser.SetRole(EUserRole.Admin);
            db.Users.Add(adminUser);
            await db.SaveChangesAsync();
            logger.LogInformation("Usuário administrador padrão criado com sucesso.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao criar usuário administrador padrão.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors();
}

var rawUploadPath = builder.Configuration["FileStorage:LocalPath"];
var uploadPath = !string.IsNullOrEmpty(rawUploadPath) && Path.IsPathRooted(rawUploadPath)
    ? rawUploadPath
    : Path.Combine(Path.GetTempPath(), "guiaeventos", "uploads");
Directory.CreateDirectory(uploadPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
await app.RunAsync();
