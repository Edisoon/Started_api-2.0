# Guia de uso de endpoints en Swagger

Esta guia explica el orden recomendado para probar la API desde Swagger.

Base URL local:

```text
http://localhost:5283
```

Swagger:

```text
http://localhost:5283/swagger
```

## 0. Preparar base de datos

Ejecuta la migracion:

```powershell
dotnet ef database update --project src/StartedApi.Infrastructure --startup-project src/StartedApi.Api
```

Luego inicia la API:

```powershell
dotnet run --project src/StartedApi.Api --launch-profile http
```

## 1. Registrar usuario

Endpoint:

```http
POST /api/auth/register
```

Body:

```json
{
  "email": "edisonlopez1992@gmail.com",
  "password": "M3ka201192!",
  "confirmPassword": "M3ka201192!",
  "firstName": "Edison",
  "lastName": "Lopez"
}
```

Resultado esperado:

```json
{
  "success": true,
  "message": "User registered successfully.",
  "data": {
    "message": "User registered successfully. Confirm email before login."
  },
  "errors": []
}
```

Si el usuario ya existe, es normal recibir `400 Bad Request`.

## 2. Confirmar email

Endpoint:

```http
POST /api/auth/confirm-email
```

Body:

```json
{
  "userId": "GUID_DEL_USUARIO",
  "token": "TOKEN_DE_CONFIRMACION"
}
```

Importante: en la version actual, la API genera el mecanismo de confirmacion usando Identity, pero todavia no envia el token por email ni lo devuelve en modo desarrollo. Para pruebas locales, confirma el usuario directamente en SQL Server:

```sql
UPDATE AspNetUsers
SET EmailConfirmed = 1
WHERE Email = 'edisonlopez1992@gmail.com';
```

Sin `EmailConfirmed = 1`, el login falla porque `RequireConfirmedEmail` esta activo.

## 3. Login

Endpoint:

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "edisonlopez1992@gmail.com",
  "password": "M3ka201192!"
}
```

Resultado esperado:

```json
{
  "success": true,
  "message": "",
  "data": {
    "accessToken": "JWT_ACCESS_TOKEN",
    "refreshToken": "REFRESH_TOKEN",
    "expiresAtUtc": "2026-07-01T00:00:00Z",
    "user": {
      "id": "GUID_DEL_USUARIO",
      "email": "edisonlopez1992@gmail.com",
      "userName": "edisonlopez1992@gmail.com",
      "firstName": "Edison",
      "lastName": "Lopez",
      "emailConfirmed": true,
      "roles": ["User"]
    }
  },
  "errors": []
}
```

Copia el `accessToken`.

## 4. Autorizar Swagger

En Swagger:

1. Clic en `Authorize`.
2. Pega solo el valor del JWT, sin escribir `Bearer`:

```text
JWT_ACCESS_TOKEN
```

3. Clic en `Authorize`.

Desde este punto puedes probar endpoints protegidos.

## 5. Obtener perfil propio

Endpoint:

```http
GET /api/users/me
```

Requiere header HTTP:

```text
Authorization: Bearer JWT_ACCESS_TOKEN
```

## 6. Actualizar perfil propio

Endpoint:

```http
PUT /api/users/me
```

Body:

```json
{
  "firstName": "Edison",
  "lastName": "Lopez"
}
```

## 7. Cambiar contrasena

Endpoint:

```http
POST /api/auth/change-password
```

Body:

```json
{
  "currentPassword": "M3ka201192!",
  "newPassword": "NuevaClave123!",
  "confirmPassword": "NuevaClave123!"
}
```

Despues de cambiarla, el proximo login debe usar la nueva contrasena.

## 8. Refresh token

Endpoint:

```http
POST /api/auth/refresh-token
```

Body:

```json
{
  "refreshToken": "REFRESH_TOKEN"
}
```

Resultado:

- Nuevo `accessToken`.
- Nuevo `refreshToken`.
- El refresh token anterior queda revocado.

## 9. Revocar refresh token

Endpoint:

```http
POST /api/auth/revoke-refresh-token
```

Body:

```json
{
  "refreshToken": "REFRESH_TOKEN",
  "reason": "Manual revocation from Swagger"
}
```

## 10. Logout

Endpoint:

```http
POST /api/auth/logout
```

Body:

```json
{
  "refreshToken": "REFRESH_TOKEN"
}
```

## 11. Recuperacion de contrasena

Endpoint:

```http
POST /api/auth/forgot-password
```

Body:

```json
{
  "email": "edisonlopez1992@gmail.com"
}
```

Importante: en la version actual se genera el token internamente, pero no se envia por email ni se devuelve. Para completar este flujo profesionalmente falta implementar `IEmailSender`.

Endpoint para reset:

```http
POST /api/auth/reset-password
```

Body:

```json
{
  "email": "edisonlopez1992@gmail.com",
  "token": "TOKEN_DE_RESET",
  "newPassword": "NuevaClave123!",
  "confirmPassword": "NuevaClave123!"
}
```

## 12. Habilitar usuario Admin para endpoints administrativos

Los endpoints de usuarios y roles administrativos requieren rol `Admin`.

Como todavia no hay pantalla ni endpoint publico para crear el primer administrador, asigna el rol manualmente en SQL Server para pruebas locales:

```sql
DECLARE @UserId uniqueidentifier;
DECLARE @RoleId uniqueidentifier;

SELECT @UserId = Id
FROM AspNetUsers
WHERE Email = 'edisonlopez1992@gmail.com';

SELECT @RoleId = Id
FROM AspNetRoles
WHERE NormalizedName = 'ADMIN';

IF @UserId IS NOT NULL
   AND @RoleId IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM AspNetUserRoles
       WHERE UserId = @UserId AND RoleId = @RoleId
   )
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END
```

Luego vuelve a iniciar sesion para que el JWT incluya el rol `Admin`.

## 13. Listar usuarios

Endpoint:

```http
GET /api/users
```

Parametros opcionales en Swagger:

```text
pageNumber: 1
pageSize: 10
searchTerm: edison
isActive: true
role: Admin
```

Requiere rol:

```text
Admin
```

## 14. Obtener usuario por ID

Endpoint:

```http
GET /api/users/{id}
```

Parametro:

```text
id: GUID_DEL_USUARIO
```

## 15. Activar o desactivar usuario

Endpoint:

```http
PATCH /api/users/{id}/status
```

Body:

```json
{
  "isActive": false,
  "reason": "Disabled from Swagger test"
}
```

Para activarlo de nuevo:

```json
{
  "isActive": true,
  "reason": "Enabled from Swagger test"
}
```

## 16. Crear rol

Endpoint:

```http
POST /api/roles
```

Body:

```json
{
  "name": "Manager",
  "description": "Management role"
}
```

## 17. Listar roles

Endpoint:

```http
GET /api/roles
```

## 18. Asignar rol a usuario

Endpoint:

```http
POST /api/roles/assign
```

Body:

```json
{
  "userId": "GUID_DEL_USUARIO",
  "roleName": "Manager"
}
```

Despues de asignar o quitar roles, el usuario debe volver a iniciar sesion para que el JWT refleje los cambios.

## 19. Quitar rol a usuario

Endpoint:

```http
POST /api/roles/remove
```

Body:

```json
{
  "userId": "GUID_DEL_USUARIO",
  "roleName": "Manager"
}
```

## 20. Consultar roles de un usuario

Endpoint:

```http
GET /api/roles/users/{userId}
```

Parametro:

```text
userId: GUID_DEL_USUARIO
```

## Orden recomendado completo

1. `POST /api/auth/register`
2. Confirmar email manualmente en DB o implementar envio de email.
3. `POST /api/auth/login`
4. Clic en `Authorize` y pegar `Bearer JWT_ACCESS_TOKEN`.
5. `GET /api/users/me`
6. `PUT /api/users/me`
7. `POST /api/auth/change-password`
8. `POST /api/auth/refresh-token`
9. Asignar rol `Admin` manualmente para pruebas locales.
10. Hacer login de nuevo.
11. `GET /api/users`
12. `GET /api/users/{id}`
13. `PATCH /api/users/{id}/status`
14. `POST /api/roles`
15. `GET /api/roles`
16. `POST /api/roles/assign`
17. `GET /api/roles/users/{userId}`
18. `POST /api/roles/remove`
19. `POST /api/auth/revoke-refresh-token`
20. `POST /api/auth/logout`
