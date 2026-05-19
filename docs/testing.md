# Pruebas y cobertura

El proyecto incluye pruebas automatizadas para backend y frontend. La meta general es mantener cobertura mínima de 90% y branch coverage mínima de 85%.

## Backend

Ubicación:

```text
backend/tests/
  Application.UnitTests/
  API.IntegrationTests/
```

Las pruebas unitarias cubren:

- Validadores de Application.
- Servicios de Application.
- Casos exitosos.
- Errores de validación.
- Duplicados.
- Entidades no encontradas.
- Reglas de negocio.
- Autenticación y administración de usuarios.

Las pruebas de integración cubren endpoints principales con servidor de pruebas y base aislada, sin depender de `Infrastructure/app.db`.

Ejecutar pruebas:

```powershell
cd backend
dotnet test .\MonorepoBackend.sln
```

Ejecutar cobertura:

```powershell
cd backend
powershell -ExecutionPolicy Bypass -File .\test-coverage.ps1
```

El script:

1. Limpia resultados anteriores.
2. Restaura la solución.
3. Ejecuta `dotnet test` con `XPlat Code Coverage`.
4. Restaura herramientas locales.
5. Genera reporte con ReportGenerator.
6. Falla si line coverage es menor a 90% o branch coverage es menor a 85%.

Salida del reporte:

```text
backend/coverage-report/
```

## Frontend

Ubicación:

```text
frontend/src/**/*.test.ts
frontend/src/**/*.test.tsx
frontend/src/test/setup.ts
```

Las pruebas cubren:

- App y flujos principales.
- AuthProvider.
- Cliente HTTP.
- Almacenamiento de tokens.
- Helpers de roles.
- Hooks reutilizables.
- Confirmaciones.
- Manejo de errores y refresh token.
- Rutas protegidas y navegación por roles.

Ejecutar pruebas:

```powershell
cd frontend
npm run test
```

Modo watch:

```powershell
npm run test:watch
```

Coverage:

```powershell
npm run test:coverage
```

Build:

```powershell
npm run build
```

Umbrales en `frontend/vite.config.ts`:

- Statements: 90%.
- Lines: 90%.
- Branches: 85%.
- Functions: 90%.

Salida del reporte:

```text
frontend/coverage/
```

## Nota sobre Vitest en Windows/OneDrive

Los scripts de `package.json` ejecutan Vitest con:

```text
--pool=forks --poolOptions.forks.singleFork
```

Esto evita errores intermitentes de workers al ejecutar pruebas en este entorno.

## Regresión esperada

Flujos que deben permanecer cubiertos:

- Usuario sin token no puede acceder a pantallas privadas.
- Usuario con token válido carga `/api/admin/users/me`.
- Access token expirado dispara refresh.
- Refresh exitoso reintenta la petición original.
- Refresh fallido limpia sesión.
- Logout llama a la API y limpia sesión.
- Duplicados muestran errores del backend.
- Roles insuficientes ocultan vistas y producen `403` si se fuerza la llamada.
- Argon2id permanece como responsabilidad del backend.
