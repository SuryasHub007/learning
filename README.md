# Backend

ASP.NET Core Web API on .NET 10, Clean Architecture, EF Core with PostgreSQL. This project contains architecture only, with no business features, entities, tables or migrations.

## Prerequisites

- .NET 10 SDK
- PostgreSQL, only once features start using the database. The API starts and serves `/health` without it.

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/Backend.API --launch-profile http
```

| URL | Purpose |
| --- | --- |
| http://localhost:5295/health | Health check (process liveness; does not touch the database) |
| http://localhost:5295/swagger | Swagger UI (Development only) |

## Test

```bash
dotnet test
```

## Project layout

| Project | Contains | May depend on |
| --- | --- | --- |
| `Backend.Domain` | Entities, enums, value objects, domain exceptions, `IRepository<T>`, `IUnitOfWork` | Nothing |
| `Backend.Application` | DTOs, service interfaces, common models | Domain |
| `Backend.Infrastructure` | `ApplicationDbContext`, EF configurations, repository and external service implementations | Application, Domain |
| `Backend.API` | Controllers, middleware, `ApiResponse`, DI composition, `Program.cs` | Application, Infrastructure |

Unit tests fail if Domain or Application gain a reference to an outer layer.

## Configuration

Settings load from `appsettings.json`, then `appsettings.{ASPNETCORE_ENVIRONMENT}.json`, then environment variables, with later sources winning. Use `__` for nesting in environment variables.

| Setting | Environment variable | Notes |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | Required: the API refuses to start without it. Only Development has a default, and it has no password. |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0`, `__1`, ... | Empty allows no cross-origin calls. |
| `Logging:LogLevel:*` | `Logging__LogLevel__Default` | Production logs to the console as JSON. |

Keep secrets out of the appsettings files. Use environment variables, or `dotnet user-secrets` locally.

### Authentication

Set the JWT secret and database password outside committed configuration:

```bash
dotnet user-secrets init --project src/Backend.API
dotnet user-secrets set "Jwt:Secret" "<long-random-secret-at-least-32-characters>" --project src/Backend.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=backend_dev;Username=postgres;Password=<postgres-password>" --project src/Backend.API
dotnet run --project src/Backend.API --launch-profile http
```

Apply the generated migration with the same database connection:

```bash
dotnet ef database update --project src/Backend.Infrastructure --startup-project src/Backend.API
```

Development-only seed accounts are created once at startup:

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@minijira.com | Admin@123 |
| ProjectManager | manager@minijira.com | Manager@123 |
| Employee | employee@minijira.com | Employee@123 |

To change a seed password, change the password in `DevelopmentDataSeeder.cs`, delete that account from PostgreSQL once, and restart the application in Development.

## API conventions

Controllers inherit `ApiControllerBase` and are served under `/api/{controller}`. Errors share the `ApiResponse` shape:

```json
{ "success": false, "message": "Validation failed.", "errors": { "name": ["The Name field is required."] }, "traceId": "..." }
```

| Exception | Status |
| --- | --- |
| `NotFoundException` | 404 |
| `DomainException` | 400 |
| Anything else | 500, with a generic message; details go to the log only |

## Recreating the solution

```bash
dotnet new sln -n Backend --format sln
dotnet new webapi -n Backend.API -o src/Backend.API --use-controllers --framework net10.0
dotnet new classlib -n Backend.Application -o src/Backend.Application --framework net10.0
dotnet new classlib -n Backend.Domain -o src/Backend.Domain --framework net10.0
dotnet new classlib -n Backend.Infrastructure -o src/Backend.Infrastructure --framework net10.0
dotnet new xunit -n Backend.UnitTests -o tests/Backend.UnitTests --framework net10.0
dotnet new xunit -n Backend.IntegrationTests -o tests/Backend.IntegrationTests --framework net10.0

dotnet sln add src/Backend.API src/Backend.Application src/Backend.Domain src/Backend.Infrastructure tests/Backend.UnitTests tests/Backend.IntegrationTests

dotnet add src/Backend.Application reference src/Backend.Domain
dotnet add src/Backend.Infrastructure reference src/Backend.Application src/Backend.Domain
dotnet add src/Backend.API reference src/Backend.Application src/Backend.Infrastructure
dotnet add tests/Backend.UnitTests reference src/Backend.Domain src/Backend.Application
dotnet add tests/Backend.IntegrationTests reference src/Backend.API

dotnet remove src/Backend.API package Microsoft.AspNetCore.OpenApi
dotnet add src/Backend.API package Swashbuckle.AspNetCore
dotnet add src/Backend.Application package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add src/Backend.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add tests/Backend.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
```