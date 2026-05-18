# MonorepoBackend

Backend .NET 8 con Clean Architecture para paises y departamentos.

## Proyectos

- `Domain`: entidades `Country` y `Department`.
- `Application`: DTOs, contratos, servicios, comandos y validadores.
- `Infrastructure`: EF Core 8, `AppDbContext`, repositorios y SQL Server.
- `API`: controladores, Swagger y CORS.

## Ejecutar

```powershell
dotnet restore .\MonorepoBackend.sln
dotnet run --project .\API\API.csproj
```

Swagger queda disponible en `/swagger` cuando `ASPNETCORE_ENVIRONMENT=Development`.

La API usa SQLite y crea la base local en `Infrastructure/app.db`.

## Pruebas y cobertura

La solución incluye:

- `tests\Application.UnitTests`: pruebas unitarias de validadores, servicios y entidades de dominio.
- `tests\API.IntegrationTests`: pruebas de integración/regresión con `WebApplicationFactory` y SQLite temporal aislado.

Ejecutar todas las pruebas:

```powershell
dotnet test .\MonorepoBackend.sln
```

Ejecutar pruebas, generar reporte y exigir cobertura mínima de 90% lineal y 85% branch:

```powershell
.\test-coverage.ps1
```

El reporte queda en `coverage-report\index.html` y el script falla si no cumple los umbrales.
