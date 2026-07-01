# Seguridad

## Identity

La API usa ASP.NET Core Identity extendiendo `IdentityUser<Guid>` e `IdentityRole<Guid>`. Esta decision evita reimplementar hashing de contrasenas, lockout, roles, tokens de confirmacion y reset de contrasena.

## JWT

Los access tokens son JWT firmados con HMAC SHA-256. Deben tener vida corta, configurada por `Jwt:AccessTokenMinutes`.

El cliente debe enviarlos asi:

```http
Authorization: Bearer {{access_token}}
```

## Refresh tokens

Los refresh tokens son aleatorios, de alta entropia y se guardan en base de datos como hash SHA-256.

Reglas:

- No se guarda el refresh token en texto plano.
- Cada refresh exitoso rota el token anterior.
- Logout revoca el refresh token.
- Tokens expirados o revocados no se aceptan.

## Cookies

Esta base prioriza JWT para clientes web SPA y moviles. Cookies seguras pueden agregarse si el proyecto usa MVC/Razor/Blazor Server o patron BFF.

## Configuracion

No hardcodear secretos reales. En produccion usar:

- Variables de entorno.
- User secrets solo en desarrollo.
- Azure Key Vault u otro proveedor seguro.

## Auditoria

Se auditan eventos como login, logout, cambios de contrasena, actualizacion de usuarios y cambios de roles. No se deben auditar contrasenas, hashes ni tokens sin hash.
