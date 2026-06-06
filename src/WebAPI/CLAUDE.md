# WebAPI

Camada de apresentação. Expõe a API REST usando ASP.NET Core Minimal APIs.

## Dependências

- `Microsoft.AspNetCore.OpenApi` 10.0.5 — documentação OpenAPI/Swagger
- `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` 1.23.0 — suporte Docker no VS
- Projeto `Repository`

## Configuração (Program.cs)

O `Program.cs` segue o modelo de Minimal API. Organizar em seções:

```
1. Builder — registrar serviços (DI)
2. Middleware pipeline — usar, mapear rotas
3. app.Run()
```

### Registro do DbContext

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    ));
```

### Connection string (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=guiaeventos;User=root;Password=senha;"
  }
}
```

Nunca commitar credenciais reais. Usar `appsettings.Development.json` (ignorado no git) ou variáveis de ambiente.

## Estrutura de endpoints

Organizar endpoints em arquivos separados por recurso usando extension methods sobre `IEndpointRouteBuilder`:

```
WebAPI/
├── Endpoints/
│   ├── PlacesEndpoints.cs
│   ├── ServicesEndpoints.cs
│   └── CategoriesEndpoints.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

Registrar no `Program.cs`:

```csharp
app.MapPlacesEndpoints();
app.MapServicesEndpoints();
```

## Padrão de endpoint

```csharp
public static class PlacesEndpoints
{
    public static IEndpointRouteBuilder MapPlacesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/places").WithTags("Places");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }
}
```

## Convenções

- Remover o endpoint `/weatherforecast` do template — é placeholder
- Prefixo de rotas: `/api/{recurso}` no plural
- Retornar `Results.Ok`, `Results.Created`, `Results.NotFound`, `Results.BadRequest` — nunca retornar tipos de domínio diretamente na resposta HTTP
- Usar DTOs/ViewModels para request e response — isolar o contrato HTTP das entidades de domínio
- Validação de request com `IValidator<T>` (FluentValidation) ou `IEndpointFilter`
- Soft delete: endpoint DELETE não deleta fisicamente — chama lógica que seta `IsDeleted = true`

## Docker

O `Dockerfile` usa multi-stage build:

- **base**: `mcr.microsoft.com/dotnet/aspnet:10.0` — imagem de runtime
- **build**: `mcr.microsoft.com/dotnet/sdk:10.0` — compilação
- Portas expostas: `8080` (HTTP) e `8081` (HTTPS)

Connection string em container deve ser passada via variável de ambiente:

```bash
docker run -e ConnectionStrings__Default="..." guiaeventos_api
```

## Executar localmente

```bash
dotnet run --project src/WebAPI
# Swagger disponível em: https://localhost:{porta}/swagger
```
