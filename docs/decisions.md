# Architecture decisions

## .NET 10 LTS

The template targets .NET 10 and pins the SDK feature band in `global.json`. Upgrade to a new LTS as a deliberate template release.

## Four production projects

The four projects make dependency direction a compiler-enforced boundary while keeping the template familiar for .NET technical tests.

## Services first, one handler as reference

CRUD uses a feature application service. `ChangeProductPriceHandler` demonstrates a Command Handler without forcing every small operation into its own file.

## No MediatR

MediatR is not required by Clean Architecture. It adds an in-process mediator and pipeline behavior mechanism. This template keeps dispatch explicit until a real cross-cutting need justifies a mediator.

## PostgreSQL and Testcontainers

PostgreSQL is the reference database. Integration tests use the same database engine in Testcontainers; EF InMemory is intentionally not used for persistence tests.

## Result plus Problem Details

Expected business errors are values. Unexpected faults are exceptions handled centrally. This avoids using exceptions for ordinary not-found, conflict and validation flows.

## JWT local authentication

Identity, JWT access tokens and rotating refresh tokens provide a realistic but replaceable local auth implementation. External identity providers can be added behind the Application auth port later.

## Opinionated template

The first version has one supported shape instead of combinatorial `dotnet new` options. Features such as caching, MediatR and external OAuth are intentionally not included.
