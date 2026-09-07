# Authentication

The template includes local authentication as an Infrastructure adapter.

## Endpoints

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/revoke
GET  /api/auth/me
```

## Security decisions

- ASP.NET Core Identity stores users, password hashes and roles in PostgreSQL.
- Registration creates only the `User` role; clients cannot request `Admin`.
- Access JWTs are short-lived by default (15 minutes).
- Refresh tokens are random 64-byte values. Only SHA-256 hashes are stored.
- Refresh tokens rotate on every successful refresh.
- Reuse of a revoked refresh token revokes every active token in its family.
- JWT issuer, audience and signing key are validated.
- Production signing keys must come from user secrets, environment variables or a secret manager.
- The development signing key in `appsettings.Development.json` is not a production secret.

## Configuration

For local development:

```bash
./scripts/dotnet10.sh user-secrets init \
  --project src/Company.ProjectName.Api

./scripts/dotnet10.sh user-secrets set Jwt:SigningKey \
  "a-real-local-development-key-with-at-least-64-random-characters" \
  --project src/Company.ProjectName.Api
```

For deployment, use environment variables such as:

```text
Jwt__Issuer=your-api
Jwt__Audience=your-clients
Jwt__SigningKey=<secret-manager-value>
```

Never commit a real signing key, password or refresh token.
