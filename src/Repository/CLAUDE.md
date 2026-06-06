# Repository

Camada de infraestrutura/persistência. Implementa acesso ao banco de dados usando Entity Framework Core com MySQL.

## Dependências

- `Microsoft.EntityFrameworkCore` 10.0.8
- `Microsoft.EntityFrameworkCore.Tools` 10.0.8
- Projeto `Domain`

O provider MySQL deve ser adicionado (ex: `Pomelo.EntityFrameworkCore.MySql`).

## Estrutura

```
Repository/
└── Persistence/
    ├── AppDbContext.cs     # DbContext principal da aplicação
    └── Maps/
        ├── EntityMap.cs        # Mapeamento base para Entity<T>
        ├── AdvertisementMap.cs # Mapeamento de Advertisement (stub)
        ├── AuthorMap.cs        # Mapeamento de Author (stub)
        ├── BannerMap.cs        # Mapeamento de Banner (stub)
        └── CategoryMap.cs      # Mapeamento de Category (stub)
```

## AppDbContext

`AppDbContext` recebe `DbContextOptions<AppDbContext>` via construtor (injeção de dependência).

`DbSet<T>` a adicionar conforme entidades forem mapeadas:

```csharp
public DbSet<Place> Places => Set<Place>();
public DbSet<Service> Services => Set<Service>();
public DbSet<Category> Categories => Set<Category>();
public DbSet<User> Users => Set<User>();
```

Chamar `modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())` no `OnModelCreating` para auto-registrar todos os `IEntityTypeConfiguration<T>`.

## Mapeamentos (Fluent API)

Todo mapeamento implementa `IEntityTypeConfiguration<T>` e fica em `Persistence/Maps/`.

### Padrão de herança

- `EntityMap<T>` é classe base interna (`internal`) que configura os campos de `Entity`
- Mapas concretos herdam de `EntityMap<T>` e chamam `base.ConfigureProperties(builder)` antes de configurar propriedades específicas

### Atenção: inconsistência existente

`EntityMap.cs` referencia `IsActive` mas `Entity.cs` declara `IsDeleted`. Corrigir antes de gerar migrations:

```csharp
// ERRADO (atual)
builder.Property(x => x.IsActive).IsRequired();

// CORRETO
builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
```

### Soft delete — query filter global

Configurar no `AppDbContext.OnModelCreating` um query filter global para excluir registros deletados:

```csharp
modelBuilder.Entity<Place>().HasQueryFilter(x => !x.IsDeleted);
```

### Address (owned entity)

`Address` é Value Object e deve ser mapeado como owned entity no mapa de `Place`:

```csharp
builder.OwnsOne(x => x.Location, address =>
{
    address.Property(a => a.Street).HasMaxLength(200).IsRequired();
    address.Property(a => a.City).HasMaxLength(100).IsRequired();
    // ...
});
```

### TPH vs TPT para Advertisement

A hierarquia `Advertisement → Place / Service` pode usar:
- **TPH** (Table Per Hierarchy) — uma única tabela `Advertisements` com coluna discriminadora. Simples, padrão do EF.
- **TPT** (Table Per Type) — tabela separada por tipo. Usar se Place e Service tiverem muitas colunas distintas.

Decisão padrão: **TPH** enquanto o modelo for simples.

## Migrations

Migrations ficam no projeto Repository. Comandos executados da raiz da solution:

```bash
# Adicionar migration
dotnet ef migrations add <Nome> --project src/Repository --startup-project src/WebAPI

# Aplicar ao banco
dotnet ef database update --project src/Repository --startup-project src/WebAPI

# Reverter última migration
dotnet ef migrations remove --project src/Repository --startup-project src/WebAPI
```

## Convenções

- Toda configuração de banco via Fluent API — não usar DataAnnotations nas entidades para mapeamento
- Nomes de tabelas no plural, snake_case (convenção MySQL): `places`, `services`, `categories`
- Configurar `HasMaxLength` em todas as strings para evitar colunas `longtext` desnecessárias
- Índices explícitos em campos usados em filtros frequentes (ex: `IsDeleted`, `CreatedBy`)
- Nunca expor repositórios genéricos — preferir interfaces específicas por agregado
