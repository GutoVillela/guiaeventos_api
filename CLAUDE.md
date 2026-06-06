# guiaeventos_api

Sistema de controle de anúncios para um portal de eventos. Permite cadastrar e gerenciar dois tipos de anúncios: **Espaços (Places)** e **Serviços (Services)**.

## Arquitetura

Clean Architecture em três camadas, todas targeting **.NET 10.0**:

```
src/
├── Domain/      # Entidades e regras de negócio (sem dependências externas)
├── Repository/  # Acesso a dados com EF Core + MySQL
└── WebAPI/      # API REST com ASP.NET Core Minimal APIs
```

Dependências seguem sentido único: `WebAPI → Repository → Domain`.

## Stack Técnica

- **Runtime**: .NET 10.0
- **API**: ASP.NET Core Minimal APIs
- **ORM**: Entity Framework Core 10.0.8
- **Banco de dados**: MySQL
- **Documentação**: OpenAPI (Swagger) via `Microsoft.AspNetCore.OpenApi`
- **Containerização**: Docker (multi-stage build, portas 8080/8081)

## Estrutura da solução

```
guiaeventos_api.slnx   # Solution file
src/
  Domain/
  Repository/
  WebAPI/
```

## Convenções do projeto

- Nullable reference types habilitado em todos os projetos (`<Nullable>enable</Nullable>`)
- Implicit usings habilitado (`<ImplicitUsings>enable</ImplicitUsings>`)
- Soft delete via `IsDeleted` — nunca deletar registros fisicamente
- Campos de auditoria obrigatórios em toda entidade: `CreatedAt`, `CreatedBy`, `UpdatedAt`
- Value Objects implementados como `record` imutáveis no Domain
- Fluent API do EF Core para todos os mapeamentos (sem DataAnnotations no Repository)
- Migrations geradas no projeto Repository

## Migrações EF Core

```bash
# Criar migration (rodar da raiz da solution)
dotnet ef migrations add <NomeDaMigration> --project src/Repository --startup-project src/WebAPI

# Aplicar migrations
dotnet ef database update --project src/Repository --startup-project src/WebAPI
```

## Executar o projeto

```bash
dotnet run --project src/WebAPI
```

## Observações de implementação

- `EntityMap<T>` referencia `IsActive` mas a entidade base usa `IsDeleted` — corrigir antes de gerar a primeira migration
- `AppDbContext` ainda não possui `DbSet<T>` configurados
- Endpoints de `/weatherforecast` no WebAPI são placeholder do template e devem ser removidos
