using WebAPI.Exceptions;
using Presentation;
using Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add services from other projects
string? connectionString = builder.Configuration.GetConnectionString("MySQL");

if (string.IsNullOrEmpty(connectionString))
{
    throw new EmptyConnectionStringException();
}

builder.Services
    .AddRepository(connectionString)
    .AddPresentation()
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
