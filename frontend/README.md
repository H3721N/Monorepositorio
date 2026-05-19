# Frontend

Frontend administrativo en React + TypeScript + Vite para consumir la API .NET 8 del monorepo.

## Tecnologías

- React 18.
- TypeScript.
- Vite.
- React Router.
- Vitest.
- React Testing Library.
- user-event.
- jest-dom.
- jsdom.
- Coverage con V8.

## Estructura

```text
frontend/
  src/
    auth/            AuthProvider y estado de sesión.
    components/      Layout, botones y componentes reutilizables.
    hooks/           Hooks compartidos.
    pages/           Pantallas administrativas.
    routes/          Configuración de rutas y ProtectedRoute.
    services/api/    Cliente HTTP, endpoints y almacenamiento de tokens.
    test/            Setup de pruebas.
    types/           Tipos de la API.
    utils/           Utilidades como mapeo de roles.
```

## Configuración de API

Por defecto el cliente usa:

```text
http://localhost:5080
```

Puede sobrescribirse con:

```text
VITE_API_BASE_URL=http://localhost:5080
```

El valor se lee en `src/services/api/httpClient.ts`.

## Ejecutar

```powershell
npm install
npm run dev
```

URL local:

```text
http://localhost:5173
```

Para compilar:

```powershell
npm run build
```

## Autenticación

La pantalla de login envía `email` y `password` a:

```text
POST /api/Auth/login
```

El frontend guarda `accessToken` y `refreshToken` en `localStorage` bajo la clave `monorepo.tokens`, agrega `Authorization: Bearer {accessToken}` a requests protegidos y ejecuta refresh automático cuando el backend responde `401`.

Si el refresh falla, se limpia la sesión local y el usuario vuelve al login.

Argon2id no se implementa en frontend. El hashing de contraseñas corresponde al backend.

## Rutas y roles

El frontend protege pantallas privadas con rutas protegidas y muestra navegación según roles:

- `COUNTRY`: países.
- `DEPARTMENT`: departamentos.
- `USER_ADMIN`: usuarios.

El mapeo de nombres de rol a `roleIds` para formularios administrativos está centralizado en `src/utils/roles.ts`. Si se recrea la base local y los IDs cambian, ese archivo es el punto a revisar.

## Pantallas

- Login.
- Dashboard.
- Países: listar, crear, editar y eliminar.
- Departamentos: listar, filtrar por país, crear, editar y eliminar.
- Usuarios: crear usuario, cambiar roles y desactivar usuario.
- Cuenta: cambio de contraseña.

## Scripts

```powershell
npm run dev
npm run build
npm run preview
npm run test
npm run test:watch
npm run test:coverage
```

Los scripts de pruebas usan `--pool=forks --poolOptions.forks.singleFork` para evitar salidas inesperadas de workers en este entorno Windows/OneDrive.

## Pruebas y cobertura

```powershell
npm run test
npm run test:coverage
```

Umbrales configurados en `vite.config.ts`:

- Statements: 90%.
- Lines: 90%.
- Branches: 85%.
- Functions: 90%.

El reporte HTML se genera en `coverage/`.

## Problemas comunes

- `401` después de iniciar sesión: verificar que el backend esté en `http://localhost:5080` y que el token no haya expirado.
- `403 Forbidden`: el usuario autenticado no tiene el rol requerido por el endpoint.
- Opciones de usuario sin efecto después de regenerar base: revisar IDs en `src/utils/roles.ts`.
- Tests con error `Worker exited unexpectedly`: usar los scripts existentes de `package.json`, que ya fuerzan un solo fork.
