# Architecture

## Dependency direction

```text
Api ────────────────┐
                    ├──> Application ───> Domain
Infrastructure ────┘
```

- `Domain` has no project or framework dependencies.
- `Application` contains use cases, contracts, ports and response models.
- `Infrastructure` implements outbound ports and owns EF Core, PostgreSQL, Identity and JWT issuance.
- `Api` owns HTTP concerns, controllers, request DTOs, middleware, OpenAPI and composition.
- `Api` references Infrastructure only from the composition root (`Program.cs`). API features must not import Infrastructure types.

The dependency rules are executable in `tests/Company.ProjectName.ArchitectureTests` with ArchUnitNET.

## Feature organization

Features are grouped inside each project:

```text
Application/Products/
Infrastructure/Products/
Api/Features/Products/
Domain/Products/
```

This preserves project boundaries while keeping related code discoverable.

## Application service versus command handler

CRUD operations use `ProductService`, a service with several related methods. `ChangeProductPriceHandler` intentionally demonstrates the alternative Command Handler style:

```text
ChangeProductPriceCommand -> ChangeProductPriceHandler
```

It is called directly by the controller. There is no MediatR. This example is also not a claim that the solution implements full CQRS; it demonstrates one use case per class.

## Persistence ports

Application defines `IProductRepository` with operations required by its use cases. Infrastructure implements it with EF Core. Application never receives `DbContext`, `DbSet`, `IQueryable` or Npgsql types.

## Error flow

Expected business outcomes use `Result`/`Result<T>`:

```text
Domain/Application Result -> API Problem Details (RFC 7807)
Unexpected exception      -> GlobalExceptionHandler -> 500 Problem Details
```
