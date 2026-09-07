# Adding a feature

1. Define domain behavior in `src/Company.ProjectName.Domain/<Feature>`.
2. Add the feature's repository port and application contracts in `src/Company.ProjectName.Application/<Feature>`.
3. Add or extend the application service. Use a direct Command Handler when one operation deserves its own use-case class; do not add MediatR automatically.
4. Implement the port in `src/Company.ProjectName.Infrastructure/<Feature>`.
5. Add EF mapping in `Infrastructure/Persistence/Configurations` and create an explicit migration.
6. Add request/response DTOs and a controller under `src/Company.ProjectName.Api/Features/<Feature>`.
7. Add Domain and Application unit tests.
8. Add PostgreSQL/API integration tests when persistence or HTTP behavior changes.
9. Run ArchitectureTests and update documentation if the dependency graph changes.

Keep API DTOs in Api, domain entities in Domain, and infrastructure-specific types in Infrastructure. Do not move a repository implementation into Application just to reduce file navigation.
