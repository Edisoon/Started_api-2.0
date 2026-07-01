# API Reference

| Metodo | Ruta | Auth | Roles | Descripcion |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | No | Publico | Registra usuario |
| POST | `/api/auth/confirm-email` | No | Publico | Confirma email, opcional en la configuracion actual |
| POST | `/api/auth/login` | No | Publico | Inicia sesion |
| POST | `/api/auth/logout` | Si | Usuario | Revoca refresh token |
| POST | `/api/auth/refresh-token` | No | Publico | Rota refresh token y emite access token |
| POST | `/api/auth/revoke-refresh-token` | Si | Usuario/Admin | Revoca refresh token |
| POST | `/api/auth/forgot-password` | No | Publico | Solicita recuperacion |
| POST | `/api/auth/reset-password` | No | Publico | Restablece contrasena |
| POST | `/api/auth/change-password` | Si | Usuario | Cambia contrasena |
| GET | `/api/users/me` | Si | Usuario | Perfil propio |
| PUT | `/api/users/me` | Si | Usuario | Actualiza perfil propio |
| GET | `/api/users` | Si | Admin | Lista usuarios |
| GET | `/api/users/{id}` | Si | Admin | Obtiene usuario |
| PATCH | `/api/users/{id}/status` | Si | Admin | Activa/desactiva usuario |
| POST | `/api/roles` | Si | Admin | Crea rol |
| GET | `/api/roles` | Si | Admin | Lista roles |
| PUT | `/api/roles/{id}` | Si | Admin | Edita nombre y descripcion de rol |
| PATCH | `/api/roles/{id}/status` | Si | Admin | Activa/inactiva rol sin eliminarlo |
| POST | `/api/roles/assign` | Si | Admin | Asigna rol |
| POST | `/api/roles/remove` | Si | Admin | Quita rol |
| GET | `/api/roles/users/{userId}` | Si | Admin | Roles de usuario |

Las respuestas usan:

```json
{
  "success": true,
  "message": "",
  "data": {},
  "errors": []
}
```
