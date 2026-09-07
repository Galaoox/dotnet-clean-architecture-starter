# dotnet-clean-architecture-starter

Plantilla de Web API con Clean Architecture para .NET 10 (LTS) que genera una
solución completa lista para proyectos personales o pruebas técnicas.

> Estado: construcción en curso. Este README se completa al final.

## Qué genera

- 4 proyectos de producción: `Api`, `Application`, `Domain`, `Infrastructure`.
- 4 proyectos de pruebas: Domain/Application unit tests, Architecture Tests,
  Integration Tests (Testcontainers + PostgreSQL real).
- Autenticación JWT local con Identity, refresh tokens con rotación, roles y políticas.
- `Result<T>` para errores esperados y Problem Details centralizado.
- OpenAPI + Scalar en desarrollo, health checks, OpenTelemetry (opcional).
- Docker Compose con PostgreSQL y perfil de observabilidad opcional.

## Uso (cuando esté publicada)

```bash
dotnet new install <ruta-o-paquete-de-esta-plantilla>
dotnet new cleanarch -n Acme.Products
```
