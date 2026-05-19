# API

Base URL local:

```text
http://localhost:5080
```

Swagger:

```text
http://localhost:5080/swagger
```

Los endpoints protegidos requieren:

```http
Authorization: Bearer {accessToken}
```

## Auth

| Método | Ruta | Auth | Body | Respuesta |
| --- | --- | --- | --- | --- |
| POST | `/api/Auth/login` | No | `{ "email": "...", "password": "..." }` | Tokens |
| POST | `/api/Auth/refresh` | No | `{ "refreshToken": "..." }` | Tokens |
| POST | `/api/Auth/logout` | Sí | Sin body | `204 No Content` |
| POST | `/api/Auth/change-password` | Sí | `{ "currentPassword": "...", "newPassword": "..." }` | `204 No Content` |

Respuesta de tokens:

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "accessTokenExpiresAt": "2026-05-18T10:00:00Z",
  "refreshTokenExpiresAt": "2026-05-25T10:00:00Z"
}
```

## Usuarios

| Método | Ruta | Rol requerido | Body | Respuesta |
| --- | --- | --- | --- | --- |
| GET | `/api/admin/users/me` | Usuario autenticado | Sin body | Usuario actual |
| POST | `/api/admin/users` | `USER_ADMIN` | `{ "email": "...", "password": "...", "roleIds": [1] }` | Usuario creado |
| DELETE | `/api/admin/users/{id}` | `USER_ADMIN` | Sin body | `204 No Content` |
| PUT | `/api/admin/users/{id}/roles` | `USER_ADMIN` | `{ "roleIds": [1, 2] }` | Usuario actualizado |

Ejemplo de usuario:

```json
{
  "id": 1,
  "email": "admin@ejemplo.com",
  "roles": ["COUNTRY", "DEPARTMENT", "USER_ADMIN"],
  "activo": true
}
```

Notas:

- La API administrativa recibe `roleIds`.
- Los nombres de rol válidos son `COUNTRY`, `DEPARTMENT` y `USER_ADMIN`.
- El backend autoriza por nombre de rol dentro del JWT.

## Países

Todos los endpoints de países requieren rol `COUNTRY`.

| Método | Ruta | Body | Respuesta |
| --- | --- | --- | --- |
| GET | `/api/Countries` | Sin body | Lista de países |
| GET | `/api/Countries/{id}` | Sin body | País con departamentos |
| POST | `/api/Countries` | `{ "name": "...", "isoCode": "..." }` | País creado |
| PUT | `/api/Countries/{id}` | `{ "name": "...", "isoCode": "..." }` | País actualizado |
| DELETE | `/api/Countries/{id}` | Sin body | `204 No Content` |

Ejemplo:

```json
{
  "id": 1,
  "name": "Guatemala",
  "isoCode": "GT"
}
```

## Departamentos

Todos los endpoints de departamentos requieren rol `DEPARTMENT`.

| Método | Ruta | Body | Respuesta |
| --- | --- | --- | --- |
| GET | `/api/Departments` | Sin body | Lista de departamentos |
| GET | `/api/Departments?countryId={id}` | Sin body | Lista filtrada por país |
| GET | `/api/Departments/{id}` | Sin body | Departamento |
| POST | `/api/Departments` | `{ "name": "...", "countryId": 1 }` | Departamento creado |
| PUT | `/api/Departments/{id}` | `{ "name": "...", "countryId": 1 }` | Departamento actualizado |
| DELETE | `/api/Departments/{id}` | Sin body | `204 No Content` |

Ejemplo:

```json
{
  "id": 1,
  "name": "Guatemala",
  "countryId": 1
}
```

## Códigos comunes

- `200 OK`: consulta o actualización exitosa.
- `201 Created`: creación exitosa.
- `204 No Content`: operación exitosa sin payload.
- `400 Bad Request`: validación o regla de negocio.
- `401 Unauthorized`: falta token o token inválido/expirado.
- `403 Forbidden`: token válido sin rol suficiente.
- `404 Not Found`: recurso inexistente.
- `409 Conflict`: conflicto como duplicados, cuando aplique.
