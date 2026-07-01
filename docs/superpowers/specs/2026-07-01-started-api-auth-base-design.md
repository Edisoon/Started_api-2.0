# Started API Auth Base Design

## 1. Especificacion tecnica

### Objetivo del sistema

Construir una API REST base reutilizable para proyectos web y moviles que necesiten usuarios, autenticacion, autorizacion por roles, manejo profesional de tokens y auditoria de acciones relevantes. La primera version debe ser funcional, segura y extensible, sin incluir complejidad que no sea necesaria para iniciar proyectos reales.

### Alcance funcional

La primera version incluye:

- Registro de usuarios.
- Confirmacion de email.
- Login con JWT access token y refresh token.
- Logout con revocacion de refresh token.
- Refresh token seguro.
- Revocacion manual de refresh tokens.
- Recuperacion y restablecimiento de contrasena.
- Cambio de contrasena autenticado.
- Consulta y actualizacion del perfil propio.
- Listado y consulta administrativa de usuarios.
- Activacion y desactivacion de usuarios.
- Creacion, listado, asignacion y remocion de roles.
- Consulta de roles por usuario.
- Auditoria de login, logout, cambio de contrasena, actualizacion de usuario, asignacion/remocion de roles, bloqueo y cambios de estado.

### Alcance no funcional

- API mantenible usando Clean Architecture practica.
- Separacion clara entre controladores, servicios, contratos, persistencia y dominio.
- Validaciones explicitas con DTOs.
- Respuestas HTTP consistentes.
- Manejo centralizado de errores.
- Logging estructurado con `ILogger`.
- Persistencia con EF Core y SQL Server como motor recomendado.
- Configuracion segura mediante `appsettings`, user secrets y variables de entorno.
- Documentacion con Swagger/OpenAPI, README, ejemplos y archivo `.http`.
- Pruebas basicas automatizadas para flujos criticos.

### Reglas de seguridad

- No exponer entidades de dominio directamente desde controladores.
- No devolver hashes, tokens internos, security stamps ni informacion sensible.
- No hardcodear secretos.
- Usar `IdentityUser` e `IdentityRole` extendidos.
- Usar hashing de contrasenas provisto por ASP.NET Core Identity.
- Bloquear cuentas por intentos fallidos usando mecanismos de Identity.
- Generar access tokens de vida corta.
- Guardar refresh tokens como hash, no en texto plano.
- Revocar refresh tokens al hacer logout o revocacion explicita.
- Validar emisor, audiencia, firma y expiracion de JWT.
- Aplicar autorizacion por roles para endpoints administrativos.
- Registrar auditoria sin almacenar contrasenas, tokens sin hash o datos sensibles.

### Decisiones tecnicas importantes

- Version recomendada: ASP.NET Core 8 LTS.
- Motivo: .NET 8 tiene soporte de largo plazo, alta compatibilidad con paquetes del ecosistema .NET y menor riesgo para una plantilla reutilizable empresarial.
- Persistencia recomendada: SQL Server con EF Core.
- Identidad: ASP.NET Core Identity extendiendo `IdentityUser<Guid>` e `IdentityRole<Guid>`.
- Tokens: JWT para access tokens y refresh tokens persistidos con hash.
- Arquitectura: Clean Architecture practica con separacion por capas y organizacion modular interna.
- Pruebas: xUnit o MSTest son validas; se recomienda xUnit por adopcion amplia en proyectos .NET modernos, pero MSTest tambien es aceptable si se prefiere consistencia con el aprendizaje actual.

## 2. Arquitectura

### Estructura de proyectos

```text
StartedApi.sln
src/
  StartedApi.Domain/
    Auth/
    Users/
    Roles/
    Audit/
    Common/
  StartedApi.Application/
    Auth/
    Users/
    Roles/
    Audit/
    Common/
    Security/
  StartedApi.Infrastructure/
    Authentication/
    Identity/
    Persistence/
    Audit/
    Security/
  StartedApi.Api/
    Controllers/
    Middleware/
    Extensions/
    Options/
tests/
  StartedApi.Tests/
docs/
  architecture.md
  api-reference.md
  security.md
StartedApi.Api.http
README.md
```

### Responsabilidades por capa

`Domain` contiene entidades y reglas base sin depender de ASP.NET Core, EF Core ni servicios externos.

`Application` contiene DTOs, contratos, casos de uso, validaciones, resultados de operacion e interfaces como `IAuthService`, `IUserService`, `IRoleService`, `ITokenService` e `IAuditService`.

`Infrastructure` implementa persistencia, Identity, EF Core, generacion de JWT, hashing de refresh tokens, auditoria y servicios que dependen de infraestructura.

`Api` expone controladores REST, middleware, configuracion de DI, autenticacion, autorizacion, Swagger y manejo HTTP.

`Tests` valida comportamientos principales del sistema mediante pruebas de servicios y endpoints.

### Flujo de autenticacion

1. El cliente envia email y contrasena a `POST /api/auth/login`.
2. La API valida credenciales con `UserManager` y `SignInManager`.
3. Si las credenciales son invalidas, Identity incrementa los intentos fallidos.
4. Si el usuario esta bloqueado, la API responde `423 Locked` o `400 Bad Request` con codigo de error consistente.
5. Si el login es correcto, se genera un JWT access token de vida corta.
6. Se genera un refresh token aleatorio, se guarda su hash en base de datos y se devuelve el token en la respuesta.
7. Se registra auditoria de login.

### Flujo de refresh tokens

1. El cliente envia refresh token a `POST /api/auth/refresh-token`.
2. La API hashea el token recibido y busca un registro activo.
3. Si el token no existe, expiro, fue revocado o fue usado indebidamente, la API rechaza la solicitud.
4. Si es valido, se revoca el refresh token anterior y se genera uno nuevo.
5. Se emite un nuevo JWT access token.
6. Se registra auditoria cuando aplique.

### Flujo de recuperacion de contrasena

1. El cliente solicita recuperacion con `POST /api/auth/forgot-password`.
2. La API genera token de restablecimiento con Identity.
3. En la primera version, el envio real por email queda detras de una interfaz `IEmailSender`.
4. En desarrollo, se registra o devuelve solo informacion controlada segun configuracion, nunca en produccion.
5. El cliente confirma el nuevo password con `POST /api/auth/reset-password`.
6. La API restablece la contrasena con `UserManager.ResetPasswordAsync`.
7. Se registra auditoria de cambio/restablecimiento de contrasena.

### Flujo de autorizacion por roles

1. El usuario inicia sesion.
2. El JWT incluye claims minimos: user id, email, user name y roles.
3. Los endpoints administrativos usan `[Authorize(Roles = "Admin")]`.
4. Los endpoints del perfil propio usan `[Authorize]`.
5. Las operaciones de roles se auditan.

## 3. Modelo de datos

### Entidades principales

`ApplicationUser` extiende `IdentityUser<Guid>` e incluye:

- `FirstName`
- `LastName`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `LastLoginAtUtc`

`ApplicationRole` extiende `IdentityRole<Guid>` e incluye:

- `Description`
- `CreatedAtUtc`

`RefreshToken` incluye:

- `Id`
- `UserId`
- `TokenHash`
- `CreatedAtUtc`
- `ExpiresAtUtc`
- `RevokedAtUtc`
- `ReplacedByTokenHash`
- `CreatedByIp`
- `RevokedByIp`
- `ReasonRevoked`

`AuditLog` incluye:

- `Id`
- `UserId`
- `Action`
- `EntityName`
- `EntityId`
- `IpAddress`
- `UserAgent`
- `OccurredAtUtc`
- `Details`

### Relaciones

- `ApplicationUser` tiene muchos `RefreshToken`.
- `ApplicationUser` se relaciona con `ApplicationRole` mediante tablas Identity.
- `AuditLog` puede apuntar opcionalmente a `ApplicationUser`.

### Indices importantes

- Indice unico en `ApplicationUser.Email` usando normalizacion de Identity.
- Indice en `RefreshToken.TokenHash`.
- Indice compuesto en `RefreshToken.UserId` y `RefreshToken.ExpiresAtUtc`.
- Indice en `AuditLog.UserId`.
- Indice en `AuditLog.OccurredAtUtc`.
- Indice en `AuditLog.Action`.

### Consideraciones para SQL Server

- Usar `uniqueidentifier` para identificadores.
- Usar `datetime2` para fechas UTC.
- Limitar longitudes de strings sensibles y campos de auditoria.
- Usar migraciones EF Core versionadas.
- Evitar cascade deletes peligrosos sobre auditoria.

## 4. Endpoints

| Metodo | Ruta | Descripcion | Auth | Roles | Request DTO | Response DTO |
| --- | --- | --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | Registra un usuario | No | Publico | `RegisterRequest` | `AuthMessageResponse` |
| POST | `/api/auth/confirm-email` | Confirma email | No | Publico | `ConfirmEmailRequest` | `AuthMessageResponse` |
| POST | `/api/auth/login` | Inicia sesion | No | Publico | `LoginRequest` | `AuthResponse` |
| POST | `/api/auth/logout` | Cierra sesion y revoca refresh token | Si | Usuario | `LogoutRequest` | `AuthMessageResponse` |
| POST | `/api/auth/refresh-token` | Renueva access token | No | Publico | `RefreshTokenRequest` | `AuthResponse` |
| POST | `/api/auth/revoke-refresh-token` | Revoca refresh token | Si | Usuario/Admin | `RevokeRefreshTokenRequest` | `AuthMessageResponse` |
| POST | `/api/auth/forgot-password` | Solicita recuperacion | No | Publico | `ForgotPasswordRequest` | `AuthMessageResponse` |
| POST | `/api/auth/reset-password` | Restablece contrasena | No | Publico | `ResetPasswordRequest` | `AuthMessageResponse` |
| POST | `/api/auth/change-password` | Cambia contrasena autenticado | Si | Usuario | `ChangePasswordRequest` | `AuthMessageResponse` |
| GET | `/api/users/me` | Obtiene perfil propio | Si | Usuario | Ninguno | `UserProfileResponse` |
| PUT | `/api/users/me` | Actualiza perfil propio | Si | Usuario | `UpdateProfileRequest` | `UserProfileResponse` |
| GET | `/api/users` | Lista usuarios | Si | Admin | `UserQueryParameters` | `PagedResponse<UserSummaryResponse>` |
| GET | `/api/users/{id}` | Obtiene usuario por id | Si | Admin | Ninguno | `UserDetailResponse` |
| PATCH | `/api/users/{id}/status` | Activa o desactiva usuario | Si | Admin | `UpdateUserStatusRequest` | `UserDetailResponse` |
| POST | `/api/roles` | Crea rol | Si | Admin | `CreateRoleRequest` | `RoleResponse` |
| GET | `/api/roles` | Lista roles | Si | Admin | Ninguno | `IReadOnlyList<RoleResponse>` |
| POST | `/api/roles/assign` | Asigna rol a usuario | Si | Admin | `AssignRoleRequest` | `AuthMessageResponse` |
| POST | `/api/roles/remove` | Quita rol a usuario | Si | Admin | `RemoveRoleRequest` | `AuthMessageResponse` |
| GET | `/api/roles/users/{userId}` | Consulta roles de usuario | Si | Admin | Ninguno | `IReadOnlyList<string>` |

## 5. Plan de implementacion por fases

### Fase 1: Configuracion inicial

- Crear solucion y proyectos.
- Configurar referencias entre capas.
- Configurar paquetes NuGet.
- Configurar Swagger, health check basico y middleware de errores.

### Fase 2: Dominio y entidades

- Crear `ApplicationUser`.
- Crear `ApplicationRole`.
- Crear `RefreshToken`.
- Crear `AuditLog`.
- Crear constantes de roles y acciones auditables.

### Fase 3: Infraestructura y persistencia

- Crear `ApplicationDbContext`.
- Configurar Identity con EF Core.
- Configurar SQL Server.
- Crear migracion inicial.
- Configurar seeding de rol Admin si se requiere.

### Fase 4: Autenticacion

- Crear DTOs de Auth.
- Crear servicios de autenticacion.
- Implementar generacion y validacion de JWT.
- Implementar refresh tokens con hash.
- Implementar confirmacion de email.
- Implementar recuperacion/restablecimiento de contrasena.

### Fase 5: Usuarios

- Crear DTOs de usuario.
- Implementar perfil propio.
- Implementar listado y consulta por id.
- Implementar activacion/desactivacion.

### Fase 6: Roles

- Crear DTOs de roles.
- Implementar creacion y listado.
- Implementar asignacion y remocion.
- Implementar consulta de roles por usuario.

### Fase 7: Auditoria

- Crear contrato `IAuditService`.
- Registrar eventos relevantes desde servicios de aplicacion.
- Asegurar que no se auditen secretos.

### Fase 8: Documentacion

- Crear README.
- Crear documentos de arquitectura y seguridad.
- Crear archivo `.http`.
- Documentar Swagger con Bearer authentication.

### Fase 9: Pruebas basicas

- Probar registro.
- Probar login.
- Probar refresh token.
- Probar cambio de contrasena.
- Probar listado de usuarios.
- Probar asignacion de roles.

## 6. Implementacion por modulos

### Auth

Resuelve registro, login, logout, refresh token, confirmacion de email y recuperacion de contrasena. Contiene DTOs, `IAuthService`, implementacion de servicio y controlador `AuthController`.

Se conecta con Identity para credenciales, con `ITokenService` para JWT, con persistencia para refresh tokens y con `IAuditService` para auditoria.

### Users

Resuelve perfil propio, actualizacion de datos, listado administrativo, consulta por id y activacion/desactivacion. Contiene DTOs, `IUserService`, implementacion y `UsersController`.

Se conecta con `UserManager<ApplicationUser>`, EF Core para consultas y `IAuditService` para eventos de actualizacion y cambios de estado.

### Roles

Resuelve creacion, listado, asignacion, remocion y consulta de roles. Contiene DTOs, `IRoleService`, implementacion y `RolesController`.

Se conecta con `RoleManager<ApplicationRole>`, `UserManager<ApplicationUser>` y auditoria.

### Audit

Resuelve el registro historico de acciones relevantes. Contiene `AuditLog`, `IAuditService` e implementacion persistente.

Se conecta desde Auth, Users y Roles. No debe depender de controladores para decidir que auditar.

### Common

Contiene tipos transversales como `ApiResponse<T>`, `PagedResponse<T>`, errores de validacion, excepciones de negocio, constantes y utilidades compartidas.

### Security

Contiene opciones de JWT, generacion de tokens, hashing de refresh tokens, resolucion de usuario actual y politicas de autorizacion.

## 7. Pruebas basicas

Las pruebas se implementaran con un `WebApplicationFactory` o pruebas de servicios segun convenga por velocidad y estabilidad. Para persistencia se usara SQLite en memoria o un proveedor de pruebas controlado; no se recomienda depender de SQL Server real en pruebas unitarias rapidas.

Casos minimos:

- Registro crea usuario y no devuelve datos sensibles.
- Login correcto devuelve access token y refresh token.
- Login invalido no devuelve tokens.
- Refresh token valido rota el token anterior.
- Refresh token revocado no puede reutilizarse.
- Cambio de contrasena autenticado actualiza credenciales.
- Usuario Admin puede listar usuarios.
- Usuario sin rol Admin no puede listar usuarios.
- Admin puede asignar rol a usuario.
- Asignacion de rol registra auditoria.

## 8. Documentacion

### README

El README debe incluir:

- Objetivo de la plantilla.
- Requisitos: .NET SDK 8, SQL Server, EF Core tools.
- Configuracion local.
- Variables de entorno necesarias.
- Comandos para migraciones.
- Comandos para ejecutar API y pruebas.
- Explicacion de roles iniciales.
- Ejemplos de login y uso de Bearer token.

### Swagger/OpenAPI

Swagger debe incluir:

- Titulo y descripcion de la API.
- Esquema de seguridad Bearer JWT.
- Agrupacion por controladores.
- Respuestas HTTP principales.

### Ejemplos y archivo HTTP

El archivo `StartedApi.Api.http` debe incluir:

- Registro.
- Confirmacion de email.
- Login.
- Refresh token.
- Perfil propio.
- Listado de usuarios.
- Creacion de rol.
- Asignacion de rol.

### Extensiones futuras

No forman parte obligatoria de la primera version:

- 2FA.
- Permisos granulares por claim o policy.
- Gestion completa de dispositivos.
- Multi-tenant.
- Rate limiting avanzado.
- Outbox para auditoria/eventos.
- Integracion real con proveedor de email.
- Cookies seguras para escenarios web server-rendered o BFF.

## Decision final

La primera version se implementara como una base profesional y funcional, centrada en seguridad, mantenibilidad y extensibilidad. La decision principal es usar ASP.NET Core Identity extendido, JWT access tokens y refresh tokens con hash, dentro de una Clean Architecture practica. Esto reduce riesgo tecnico, aprovecha buenas practicas del ecosistema .NET y deja una base clara para crecer.
