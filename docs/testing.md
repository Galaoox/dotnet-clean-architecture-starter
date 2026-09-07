# Testing

## Test projects

```text
Domain.UnitTests       Pure aggregate/invariant tests
Application.UnitTests  Services and handlers with NSubstitute ports
ArchitectureTests      Dependency direction and placement rules
IntegrationTests       PostgreSQL/Testcontainers and WebApplicationFactory
```

## Local commands

The repository can use a local .NET 10 SDK or the Docker wrapper:

```bash
./scripts/dotnet10.sh restore --locked-mode
./scripts/dotnet10.sh build Company.ProjectName.slnx --configuration Release --no-restore
./scripts/dotnet10.sh test tests/Company.ProjectName.Domain.UnitTests --configuration Release --no-build
./scripts/dotnet10.sh test tests/Company.ProjectName.Application.UnitTests --configuration Release --no-build
./scripts/dotnet10.sh test tests/Company.ProjectName.ArchitectureTests --configuration Release --no-build
TESTCONTAINERS_RYUK_DISABLED=true ./scripts/dotnet10.sh test tests/Company.ProjectName.IntegrationTests --configuration Release --no-build
```

Run test projects separately with Microsoft Testing Platform. A single invocation containing multiple projects can report zero tests for the first project.

## Integration tests

Integration tests use PostgreSQL 16 through Testcontainers and therefore require Docker. The Docker wrapper mounts the Docker socket, uses the host network, and allows disabling Ryuk in the nested SDK-container workflow. Tests explicitly dispose their containers when Ryuk is disabled.

Do not replace PostgreSQL integration tests with EF Core InMemory. InMemory does not reproduce PostgreSQL mappings, constraints or SQL behavior.

## Generated template smoke test

```bash
TMP_DIR="$(mktemp -d)"
docker run --rm --network host \
  -v "$PWD:/src" \
  -v "$TMP_DIR:/out" \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  sh -c 'dotnet new install /src --force && dotnet new cleanarch -n Sample.Api -o /out/Sample.Api && cd /out/Sample.Api && dotnet restore --locked-mode && dotnet build --configuration Release'
```
