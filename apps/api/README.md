# Backend

Backend .NET 8 del monorepo. Expone una API REST para autenticación, administración de usuarios, países y departamentos.

## Tecnologías

- .NET 8.
- ASP.NET Core Web API.
- EF Core 8.
- SQLite.
- Swagger/OpenAPI.
- JWT Bearer.
- Refresh tokens persistidos en usuario.
- Argon2id para hashing de contraseñas.
- xUnit, FluentAssertions, Moq, WebApplicationFactory, Coverlet y ReportGenerator para pruebas.

## Estructura

```text
apps/api/
  src/
    API/              Controladores, Swagger, CORS, autenticación y autorización.
    Application/      DTOs, comandos, servicios, validadores e interfaces.
    Domain/           Entidades y reglas del dominio.
    Infrastructure/   EF Core, SQLite, repositorios, UnitOfWork, hashing y seed.
  tests/
    Application.UnitTests/
    API.IntegrationTests/
  MonorepoBackend.sln
```

## Ejecutar

Desde la raíz:

```powershell
dotnet restore .\apps\api\MonorepoBackend.sln
dotnet run --project .\apps\api\src\API\API.csproj --launch-profile http
```

Desde `apps/api`:

```powershell
dotnet restore .\MonorepoBackend.sln
dotnet run --project .\src\API\API.csproj --launch-profile http
```

URLs:

- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`

## Base de datos

La base SQLite local se crea en:

```text
apps/api/src/Infrastructure/app.db
```

La API calcula esta ruta desde el `ContentRootPath` del proyecto `API`, por lo que sigue funcionando al mover el backend dentro de `apps/api/src`.

## Autenticación y roles

Roles existentes:

- `COUNTRY`: acceso a `/api/Countries`.
- `DEPARTMENT`: acceso a `/api/Departments`.
- `USER_ADMIN`: acceso a endpoints administrativos de usuarios.

Usuario inicial en desarrollo:

- Email: `admin@ejemplo.com`
- Password: `Admin123!`
- Roles: `COUNTRY`, `DEPARTMENT`, `USER_ADMIN`

El hashing de contraseñas se realiza en backend con Argon2id. El frontend solo envía la contraseña al endpoint correspondiente por HTTP.

## Endpoints principales

Auth:

- `POST /api/Auth/login`
- `POST /api/Auth/refresh`
- `POST /api/Auth/logout`
- `POST /api/Auth/change-password`

Usuarios:

- `GET /api/admin/users/me`
- `POST /api/admin/users`
- `DELETE /api/admin/users/{id}`
- `PUT /api/admin/users/{id}/roles`

Países:

- `GET /api/Countries`
- `GET /api/Countries/{id}`
- `POST /api/Countries`
- `PUT /api/Countries/{id}`
- `DELETE /api/Countries/{id}`

Departamentos:

- `GET /api/Departments`
- `GET /api/Departments?countryId={id}`
- `GET /api/Departments/{id}`
- `POST /api/Departments`
- `PUT /api/Departments/{id}`
- `DELETE /api/Departments/{id}`

Para detalle de payloads y autorización, ver [../../docs/api.md](../../docs/api.md).

## Pruebas

Desde la raíz:

```powershell
dotnet test .\apps\api\MonorepoBackend.sln
powershell -ExecutionPolicy Bypass -File .\apps\api\test-coverage.ps1
```

Desde `apps/api`:

```powershell
dotnet test .\MonorepoBackend.sln
powershell -ExecutionPolicy Bypass -File .\test-coverage.ps1
```

El reporte HTML se genera en:

```text
apps/api/coverage-report/
```

## Problemas comunes

- `401 Unauthorized` en Swagger: usar el botón `Authorize` con el access token JWT como Bearer token.
- `logout` devuelve `401`: el endpoint requiere un access token válido en el header `Authorization`.
- Roles no encontrados al crear usuario: enviar IDs que existan en la tabla `Roles`; los nombres válidos son `COUNTRY`, `DEPARTMENT` y `USER_ADMIN`.
- Error por archivo bloqueado al compilar: detener la API antes de correr `dotnet build` o `dotnet test`.
- Si se regenera la base local, los IDs autoincrementales pueden cambiar; el backend autoriza por nombre de rol, pero los payloads administrativos reciben `roleIds`.
