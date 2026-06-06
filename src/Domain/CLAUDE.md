# Domain

Camada de domínio da aplicação. Contém entidades, value objects e regras de negócio puras.

**Sem dependências externas** — o projeto Domain não referencia nenhum pacote NuGet nem outro projeto da solution.

## Estrutura

```
Domain/
├── Entities/
│   ├── Entity.cs           # Base abstrata para todas as entidades
│   ├── Advertisement.cs    # Base abstrata para anúncios (Place e Service)
│   ├── Place.cs            # Anúncio do tipo Espaço (possui localização)
│   ├── Service.cs          # Anúncio do tipo Serviço
│   ├── User.cs             # Usuário do sistema
│   ├── Author.cs           # Autor de conteúdo
│   ├── Category.cs         # Categoria de anúncios
│   ├── Banner.cs           # Banner publicitário (stub)
│   └── BannerItem.cs       # Item de banner (stub)
└── ValueObjects/
    ├── Address.cs          # Endereço completo (owned entity no EF)
    ├── Password.cs         # Hash de senha
    └── Phone.cs            # Telefone com DDD e número
```

## Hierarquia de entidades

```
Entity (abstract)
└── Advertisement (abstract)
    ├── Place       ← possui Address (Value Object)
    └── Service
```

## Entidade base (Entity)

Toda entidade herda de `Entity` e ganha automaticamente:

| Propriedade | Tipo | Descrição |
|---|---|---|
| `Id` | `int` | Chave primária |
| `CreatedAt` | `DateTimeOffset` | Data de criação (UTC) |
| `CreatedBy` | `string` | Identificador de quem criou |
| `UpdatedAt` | `DateTimeOffset?` | Data da última atualização |
| `IsDeleted` | `bool` | Soft delete (nunca deletar fisicamente) |

## Advertisement (base de anúncios)

| Propriedade | Tipo |
|---|---|
| `Name` | `string` |
| `Description` | `string` |
| `Summary` | `string` |

Propriedades com setter `private` — mutação deve ocorrer por métodos de domínio.

## Place

Estende `Advertisement` com:
- `Location` (`Address`) — Value Object do endereço, inicializado como `Address.Empty`

## Value Objects

Value Objects são `record` imutáveis. Seguir as regras:

- Propriedades `init` only
- Construtor privado para uso do EF Core
- Factory method estático `Create(...)` como ponto de entrada público
- Validação no construtor (ex: `Phone` valida formato)

### Address

Armazena endereço completo com: `Street`, `Neighborhood`, `City`, `State`, `Country`, `ZipCode`, `Number`, `Complement`, `ReferencePoint`.

Constante `Address.Empty` disponível para inicialização padrão.

No EF Core será mapeado como **owned entity** (colunas embutidas na tabela do dono).

## Convenções

- Setters de propriedades de entidade devem ser `private` — expor mutação via métodos com nome de intenção
- Nunca usar DataAnnotations para validação de regras de negócio — isso pertence ao construtor/método da entidade
- DataAnnotations presentes em `Entity.cs` são legado e devem ser migrados para Fluent API no Repository
- Não adicionar dependências ao Domain.csproj
