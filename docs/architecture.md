# Arquitectura

Started API usa una Clean Architecture practica con cuatro capas principales.

## Domain

Contiene entidades y constantes:

- `ApplicationUser`
- `ApplicationRole`
- `RefreshToken`
- `AuditLog`
- `AppRoles`
- `AuditActions`

La decision aprobada fue extender ASP.NET Core Identity, por eso el dominio conoce `IdentityUser<Guid>` e `IdentityRole<Guid>`.

## Application

Contiene contratos y DTOs. No depende de ASP.NET Core MVC ni de EF Core.

Responsabilidades:

- Definir requests y responses.
- Definir interfaces de servicios.
- Estandarizar respuestas con `OperationResult<T>`, `ApiResponse<T>` y `PagedResponse<T>`.

## Infrastructure

Implementa persistencia, Identity, JWT, refresh tokens, roles, usuarios y auditoria.

Responsabilidades:

- `ApplicationDbContext`.
- Configuracion de Identity.
- `AuthService`, `UserService`, `RoleService`.
- `TokenService` y `RefreshTokenHasher`.
- `AuditService`.

## Api

Expone la API HTTP.

Responsabilidades:

- Controladores REST.
- Configuracion JWT Bearer.
- Swagger/OpenAPI.
- Middleware centralizado de errores.
- Resolucion de usuario actual desde `HttpContext`.
