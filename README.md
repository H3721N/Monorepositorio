# Monorepo fullstack

Proyecto fullstack con backend en .NET 8 y frontend en React + TypeScript + Vite. El backend expone una API REST con autenticación JWT, refresh token, autorización por roles y persistencia en SQLite. El frontend consume esa API mediante un cliente HTTP centralizado, mantiene sesión local y protege vistas administrativas según roles.

## Tecnologías principales

- Backend: .NET 8, ASP.NET Core Web API, Clean Architecture, EF Core 8, SQLite, Swagger/OpenAPI.
- Seguridad backend: JWT Bearer, refresh token, Argon2id para hashing de contraseñas, roles `COUNTRY`, `DEPARTMENT` y `USER_ADMIN`.
- Frontend: React 18, TypeScript, Vite, React Router, CSS propio.
- Pruebas backend: xUnit, FluentAssertions, Moq, WebApplicationFactory/TestServer, Coverlet y ReportGenerator.
- Pruebas frontend: Vitest, React Testing Library, user-event, jest-dom, jsdom y coverage con V8.

## Estructura

```text
proyecto/
  backend/
    API/
    Application/
    Domain/
    Infrastructure/
    tests/
    MonorepoBackend.sln
    test-coverage.ps1
  frontend/
    src/
    package.json
    vite.config.ts
  docs/
```

## Requisitos

- .NET SDK 8.
- Node.js y npm compatibles con Vite 5.
- PowerShell para ejecutar el script de cobertura del backend en Windows.

## Ejecución local

Backend:

```powershell
cd backend
dotnet restore .\MonorepoBackend.sln
dotnet run --project .\API\API.csproj --launch-profile http
```

La API queda disponible en `http://localhost:5080` y Swagger en `http://localhost:5080/swagger`.

Frontend:

```powershell
cd frontend
npm install
npm run dev
```

El frontend queda disponible en `http://localhost:5173`.

## Usuario inicial

En ambiente `Development`, el backend inicializa la base SQLite y crea:

- Email: `admin@ejemplo.com`
- Password: `Admin123!`
- Roles: `COUNTRY`, `DEPARTMENT`, `USER_ADMIN`

## Pruebas

Backend:

```powershell
cd backend
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

Los umbrales configurados son 90% para cobertura general/líneas y 85% para ramas.

## Documentación

- [Arquitectura](docs/architecture.md)
- [Flujo de autenticación](docs/auth-flow.md)
- [API](docs/api.md)
- [Pruebas y cobertura](docs/testing.md)
- [Desarrollo local](docs/development.md)
- [Backend](backend/README.md)
- [Frontend](frontend/README.md)

## Notas de seguridad y repositorio

- Argon2id se usa únicamente en el backend.
- El frontend nunca debe manejar secretos ni hashear contraseñas.
- No versionar archivos generados o locales como `app.db`, `bin/`, `obj/`, `node_modules/` o `dist/`.
- La clave JWT de `appsettings.json` es de desarrollo; en producción debe venir de configuración segura.
