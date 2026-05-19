# Pruebas y cobertura

El proyecto incluye pruebas automatizadas para backend y frontend. La meta general es mantener cobertura minima de 90% y branch coverage minima de 85%.

## Backend

Ubicacion:

```text
apps/api/tests/
  Application.UnitTests/
  API.IntegrationTests/
```

Las pruebas unitarias cubren validadores, servicios de Application, casos exitosos, errores de validacion, duplicados, entidades no encontradas, reglas de negocio, autenticacion y administracion de usuarios.

Las pruebas de integracion cubren endpoints principales con servidor de pruebas y base aislada, sin depender de `Infrastructure/app.db`.

Ejecutar pruebas:

```powershell
dotnet test .\apps\api\MonorepoBackend.sln
```

Ejecutar cobertura:

```powershell
powershell -ExecutionPolicy Bypass -File .\apps\api\test-coverage.ps1
```

El script limpia resultados anteriores, restaura la solucion, ejecuta `dotnet test` con `XPlat Code Coverage`, genera reporte con ReportGenerator y falla si line coverage es menor a 90% o branch coverage es menor a 85%.

Salida del reporte:

```text
apps/api/coverage-report/
```

## Frontend

Ubicacion:

```text
apps/web/src/**/*.test.ts
apps/web/src/**/*.test.tsx
apps/web/src/test/setup.ts
apps/web/e2e/**/*.spec.ts
```

Las pruebas unitarias e integracion cubren:

- App y flujos principales.
- AuthProvider y Zustand.
- Cliente HTTP con Axios.
- Almacenamiento de tokens.
- Helpers de roles.
- Hooks reutilizables.
- Confirmaciones.
- Manejo de errores y refresh token.
- Rutas protegidas con React Router.
- Navegacion por roles.
- Validacion de login con Zod.

Ejecutar pruebas:

```powershell
pnpm --dir .\apps\web test
```

Modo watch:

```powershell
pnpm --dir .\apps\web test:watch
```

Coverage:

```powershell
pnpm --dir .\apps\web test:coverage
```

Smoke E2E:

```powershell
pnpm --dir .\apps\web test:e2e
```

Si Playwright fue instalado por primera vez:

```powershell
pnpm --dir .\apps\web exec playwright install chromium
```

Build:

```powershell
pnpm --dir .\apps\web build
```

Umbrales en `apps/web/vite.config.ts`:

- Statements: 90%.
- Lines: 90%.
- Branches: 85%.
- Functions: 90%.

Salida del reporte:

```text
apps/web/coverage/
```

## Nota sobre Vitest en Windows/OneDrive

Los scripts de `package.json` ejecutan Vitest con:

```text
--pool=forks --poolOptions.forks.singleFork
```

Esto evita errores intermitentes de workers al ejecutar pruebas en este entorno.

## Regresion esperada

Flujos que deben permanecer cubiertos:

- Usuario sin token no puede acceder a pantallas privadas.
- Usuario con token valido carga `/api/admin/users/me`.
- Access token expirado dispara refresh.
- Refresh exitoso reintenta la peticion original.
- Refresh fallido limpia sesion.
- Logout llama a la API y limpia sesion.
- Duplicados muestran errores del backend.
- Roles insuficientes ocultan vistas y producen `403` si se fuerza la llamada.
- Argon2id permanece como responsabilidad del backend.
