# Clean Architecture Web API Starter

Opinionated .NET 10 LTS starter for personal projects and technical assessments.

It provides four compiler-enforced project boundaries, feature-oriented folders, PostgreSQL/EF Core, Identity + JWT authentication, rotating refresh tokens, Products CRUD, unit/architecture/integration tests, OpenAPI + Scalar, health checks, OpenTelemetry, Docker Compose and a native `dotnet new` template.

## Generate a project

Install the template from this repository:

```bash
dotnet new install https://github.com/Galaoox/dotnet-clean-architecture-starter.git
dotnet new cleanarch -n Acme.Products -o Acme.Products
cd Acme.Products
dotnet restore --locked-mode
dotnet build
```

Or install from a local clone:

```bash
dotnet new install .
dotnet new cleanarch -n Acme.Products -o Acme.Products
```

The generated project replaces `Company.ProjectName` with the name supplied to `-n`.

## Included structure

```text
src/
  Company.ProjectName.Api/            Controllers, DTOs, middleware, composition root
  Company.ProjectName.Application/    Use cases, services, commands, ports, results
  Company.ProjectName.Domain/         Aggregates, invariants, domain errors
  Company.ProjectName.Infrastructure/ EF Core, PostgreSQL, Identity, JWT, adapters
tests/
  Company.ProjectName.Domain.UnitTests
  Company.ProjectName.Application.UnitTests
  Company.ProjectName.ArchitectureTests
  Company.ProjectName.IntegrationTests
```

See [architecture](docs/architecture.md) for dependency rules and [adding a feature](docs/adding-a-feature.md) for the workflow.

## Run with Docker

The repository includes a multi-stage API image and PostgreSQL Compose setup:

```bash
cp .env.example .env
docker compose up --build
```

The API is exposed at `http://localhost:8080`. OpenAPI JSON is available at `/openapi/v1.json`; Scalar is available at `/scalar` in Development. Health endpoints are `/health/live` and `/health/ready`.

The API does not apply migrations automatically. Create/update the database explicitly:

```bash
docker compose up -d postgres
./scripts/dotnet10.sh ef database update \
  --project src/Company.ProjectName.Infrastructure \
  --startup-project src/Company.ProjectName.Api
```

The script runs the .NET 10 SDK inside Docker. It mounts the Docker socket because integration tests use Testcontainers. Treat access to that socket as root-equivalent and use the script only in a trusted local repository.

## Authentication

The API includes:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/revoke
GET  /api/auth/me
```

Users register with the `User` role. `Admin` is not publicly assignable. Access tokens are short-lived and refresh tokens rotate; reuse of a revoked token revokes its token family. Read [authentication](docs/authentication.md) before deploying.

## Testing

With a local .NET 10 SDK:

```bash
dotnet restore --locked-mode
dotnet build Company.ProjectName.slnx --configuration Release --no-restore
dotnet test tests/Company.ProjectName.Domain.UnitTests --configuration Release --no-build
dotnet test tests/Company.ProjectName.Application.UnitTests --configuration Release --no-build
dotnet test tests/Company.ProjectName.ArchitectureTests --configuration Release --no-build
TESTCONTAINERS_RYUK_DISABLED=true dotnet test tests/Company.ProjectName.IntegrationTests --configuration Release --no-build
```

Without installing .NET on the host, replace `dotnet` with `./scripts/dotnet10.sh`. Integration tests require Docker. They use PostgreSQL 16 through Testcontainers, not EF Core InMemory. Details are in [testing](docs/testing.md).

## Observability

OpenTelemetry instrumentation is vendor-neutral and disabled as an exporter until `OpenTelemetry:OtlpEndpoint` is configured. The optional Compose collector can be started with:

```bash
docker compose -f compose.yaml -f compose.observability.yaml \
  --profile observability up -d
```

See [observability](docs/observability.md).

## Design choices

- No MediatR: dispatch is explicit; one `ChangeProductPriceHandler` demonstrates a command handler without hiding control flow.
- Application owns repository interfaces; Infrastructure owns implementations.
- Expected business errors use `Result`; unexpected exceptions use global Problem Details handling.
- PostgreSQL is the reference database and migrations are explicit.
- UUID v7 identifiers, `DateTimeOffset` and UTC timestamps are used for new records.
- The template is intentionally opinionated and does not multiply `dotnet new` options.

See [architecture decisions](docs/decisions.md).

## License

MIT
