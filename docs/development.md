# Desarrollo local

Esta guía resume cómo levantar, probar y depurar el monorepo en local.

## Levantar backend

```powershell
cd backend
dotnet restore .\MonorepoBackend.sln
dotnet run --project .\API\API.csproj --launch-profile http
```

URLs:

- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`

El perfil `http` usa `ASPNETCORE_ENVIRONMENT=Development`, inicializa SQLite y habilita Swagger.

## Levantar frontend

```powershell
cd frontend
npm install
npm run dev
```

URL:

```text
http://localhost:5173
```

El frontend consume `http://localhost:5080` por defecto. Para cambiarlo:

```powershell
$env:VITE_API_BASE_URL="http://localhost:5080"
npm run dev
```

## Login inicial

```text
Email: admin@ejemplo.com
Password: Admin123!
```

Este usuario se crea al iniciar la API en ambiente de desarrollo.

## Flujo recomendado de trabajo

1. Iniciar backend.
2. Abrir Swagger y verificar `GET /api/admin/users/me` con token si se está depurando auth.
3. Iniciar frontend.
4. Iniciar sesión con el usuario admin.
5. Probar países, departamentos, usuarios y cambio de contraseña.
6. Ejecutar pruebas antes de cerrar cambios.

## Comandos útiles

Backend:

```powershell
cd backend
dotnet build .\MonorepoBackend.sln
dotnet test .\MonorepoBackend.sln
powershell -ExecutionPolicy Bypass -File .\test-coverage.ps1
```

Frontend:

```powershell
cd frontend
npm run test
npm run test:coverage
npm run build
```

## Swagger y Bearer token

1. Ejecutar login en `POST /api/Auth/login`.
2. Copiar `accessToken`.
3. Presionar `Authorize` en Swagger.
4. Pegar el token como Bearer token.
5. Ejecutar endpoints protegidos.

## Base de datos local

SQLite se guarda en:

```text
backend/Infrastructure/app.db
```

En desarrollo se crean tablas de autenticación si no existen y se asegura el usuario admin. Los archivos de base local y artefactos generados no deben versionarse.

## Problemas comunes

### `401 Unauthorized`

Puede ocurrir por token ausente, expirado o inválido. En Swagger se resuelve agregando el token con `Authorize`; en frontend el cliente intenta refresh automático.

### `403 Forbidden`

El usuario está autenticado, pero no tiene el rol que exige la política del endpoint.

### `logout` responde `401`

`POST /api/Auth/logout` requiere access token válido. Si el token expiró y no se renovó, la API no puede identificar al usuario.

### Error al crear usuario por roles inexistentes

El body espera IDs reales de la tabla `Roles`:

```json
{
  "email": "nuevo@ejemplo.com",
  "password": "User123!",
  "roleIds": [1, 2]
}
```

Los nombres válidos son `COUNTRY`, `DEPARTMENT` y `USER_ADMIN`; los IDs pueden variar si la base local fue recreada o migrada.

### Frontend muestra IDs de roles incorrectos

Revisar:

```text
frontend/src/utils/roles.ts
```

Ese archivo centraliza el mapeo usado por los formularios de usuarios.

### Vitest muestra `Worker exited unexpectedly`

Usar los scripts actuales de `package.json`. Ya están configurados con un solo fork para estabilizar la ejecución.

### Build de backend falla por archivo bloqueado

Detener la API o cerrar el proceso `dotnet` que esté usando los binarios antes de ejecutar build o tests.
