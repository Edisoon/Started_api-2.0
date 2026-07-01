# Started API

API REST base reutilizable para autenticacion, usuarios, roles, refresh tokens y auditoria usando ASP.NET Core 8, Identity, EF Core y SQL Server.

## Requisitos

- .NET SDK 8 o superior con targeting pack de .NET 8.
- SQL Server o LocalDB para desarrollo.
- EF Core tools (`dotnet ef`).

## Arquitectura

La solucion usa Clean Architecture practica:

- `StartedApi.Domain`: entidades de identidad, refresh tokens, auditoria y constantes.
- `StartedApi.Application`: DTOs, contratos, respuestas y abstracciones.
- `StartedApi.Infrastructure`: EF Core, Identity, JWT, refresh tokens, auditoria y servicios.
- `StartedApi.Api`: controladores, autenticacion JWT, Swagger y middleware.
- `StartedApi.Tests`: pruebas de integracion con SQLite en memoria.

## Configuracion

La configuracion local vive en `src/StartedApi.Api/appsettings.Development.json`.

Para ambientes reales, reemplaza estos valores con user secrets, variables de entorno o un proveedor seguro:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StartedApiDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "StartedApi",
    "Audience": "StartedApiClients",
    "Secret": "DevelopmentOnly-Minimum32Characters-Key",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  }
}
```

No uses el secreto de desarrollo en produccion.

## Base de datos

Crear o actualizar la base de datos:

```powershell
dotnet ef database update --project src/StartedApi.Infrastructure --startup-project src/StartedApi.Api
```

Crear una nueva migracion:

```powershell
dotnet ef migrations add NombreMigracion --project src/StartedApi.Infrastructure --startup-project src/StartedApi.Api
```

## Ejecutar

```powershell
dotnet run --project src/StartedApi.Api
```

Swagger queda disponible en `/swagger` en ambiente Development.

## Probar

```powershell
dotnet test StartedApi.sln
```

## Seguridad

- Password hashing lo maneja ASP.NET Core Identity.
- Login usa JWT access tokens de vida corta.
- Refresh tokens se guardan como hash SHA-256, no en texto plano.
- Los endpoints administrativos requieren rol `Admin`.
- Los endpoints de perfil requieren usuario autenticado.
- La auditoria registra acciones relevantes sin almacenar secretos.

## Endpoints principales

- `POST /api/auth/register`
- `POST /api/auth/confirm-email`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `POST /api/auth/refresh-token`
- `POST /api/auth/revoke-refresh-token`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/auth/change-password`
- `GET /api/users/me`
- `PUT /api/users/me`
- `GET /api/users`
- `GET /api/users/{id}`
- `PATCH /api/users/{id}/status`
- `POST /api/roles`
- `GET /api/roles`
- `POST /api/roles/assign`
- `POST /api/roles/remove`
- `GET /api/roles/users/{userId}`

## Extensiones futuras

- Envio real de emails.
- 2FA.
- Permisos granulares por policies/claims.
- Gestion de dispositivos.
- Multi-tenant.
- Rate limiting avanzado.
