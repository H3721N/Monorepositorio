# Monorepo fullstack

Proyecto fullstack organizado como monorepo. Mantiene el backend .NET 8 y el frontend React + TypeScript + Vite, ahora bajo `apps/`, con configuración base para pnpm workspaces, Turborepo, CI y documentación técnica.

## Tecnologías principales

- Backend: .NET 8, ASP.NET Core Web API, Clean Architecture, EF Core 8, SQLite, Swagger/OpenAPI.
- Seguridad backend: JWT Bearer, refresh token, Argon2id para hashing de contraseñas, roles `COUNTRY`, `DEPARTMENT` y `USER_ADMIN`.
- Frontend: React 19, TypeScript, Vite 6, React Router 7, TanStack Query 5, Zustand 5, Tailwind CSS 4, Zod 3, Axios 1 y componentes estilo shadcn/ui.
- Monorepo: pnpm workspaces y Turborepo.
- Pruebas backend: xUnit, FluentAssertions, Moq, WebApplicationFactory/TestServer, Coverlet y ReportGenerator.
- Pruebas frontend: Vitest, React Testing Library, user-event, jest-dom, jsdom, coverage con V8 y smoke E2E con Playwright.

## Estructura

```text
proyecto/
  .github/workflows/
  apps/
    api/
      src/
        API/
        Application/
        Domain/
        Infrastructure/
      tests/
        API.IntegrationTests/
        Application.UnitTests/
      MonorepoBackend.sln
      test-coverage.ps1
    web/
      src/
      package.json
      vite.config.ts
  packages/
    eslint-config/
    tsconfig/
    types/
    utils/
  docs/
  tools/
  package.json
  pnpm-workspace.yaml
  turbo.json
```

## Requisitos

- .NET SDK 8.
- Node.js compatible con Vite 6.
- pnpm 9.
- PowerShell para ejecutar el script de cobertura del backend en Windows.

## Ejecución local

Backend:

```powershell
dotnet restore .\apps\api\MonorepoBackend.sln
dotnet run --project .\apps\api\src\API\API.csproj --launch-profile http
```

La API queda disponible en `http://localhost:5080` y Swagger en `http://localhost:5080/swagger`.

Frontend:

```powershell
pnpm install
pnpm --dir .\apps\web dev
```

El frontend queda disponible en `http://localhost:5173`.

También se puede usar:

```powershell
pnpm dev
pnpm dev:api
pnpm dev:web
```

## Usuario inicial

En ambiente `Development`, el backend inicializa la base SQLite y crea:

- Email: `admin@ejemplo.com`
- Password: `Admin123!`
- Roles: `COUNTRY`, `DEPARTMENT`, `USER_ADMIN`

## Pruebas

Desde la raíz:

```powershell
pnpm test
pnpm test:coverage
pnpm test:e2e
pnpm build
```

Backend directo:

```powershell
dotnet test .\apps\api\MonorepoBackend.sln
powershell -ExecutionPolicy Bypass -File .\apps\api\test-coverage.ps1
```

Frontend directo:

```powershell
pnpm --dir .\apps\web test
pnpm --dir .\apps\web test:coverage
pnpm --dir .\apps\web test:e2e
pnpm --dir .\apps\web build
```

## Documentación

- [Arquitectura](docs/architecture.md)
- [Flujo de autenticación](docs/auth-flow.md)
- [API](docs/api.md)
- [Pruebas y cobertura](docs/testing.md)
- [Desarrollo local](docs/development.md)
- [Backend](apps/api/README.md)
- [Frontend](apps/web/README.md)

## Notas de seguridad y repositorio

- Argon2id se usa únicamente en el backend.
- El frontend nunca debe manejar secretos ni hashear contraseñas.
- No versionar archivos generados o locales como `app.db`, `bin/`, `obj/`, `node_modules/`, `coverage/` o `dist/`.
- La clave JWT de desarrollo está documentada en `.env.example`; en producción debe venir de configuración segura.
