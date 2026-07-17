# API REST with Minimal API (.NET 10)

## Versión de .NET
- **.NET 10** (`net10.0`)

## Requisitos en caso de usar Linux (Como en mi caso)
Además del SDK, en Arch/CachyOS necesitás el runtime de ASP.NET Core:
```bash
sudo pacman -S aspnet-runtime-10.0 aspnet-targeting-pack-10.0
```

## Cómo levantar el proyecto

```bash
dotnet restore
dotnet run
```

Swagger: `http://localhost:5191/swagger`

## Base de datos (SQLite)

La base se crea automáticamente al iniciar la app con `EnsureCreated()` en `Program.cs`.

- Archivo generado: `app.db` (en la raíz del proyecto)
- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- Si la tabla `Currencies` está vacía, se cargan monedas de ejemplo (PYG, USD, EUR) vía `DbSeeder`

No se usan migraciones EF en esta entrega: `EnsureCreated` es suficiente para SQLite local.

## API Key de prueba

Header requerido en todos los endpoints (excepto `/` y `/swagger`):

```
X-API-KEY: test-api-key-12345
```

Configurada en `appsettings.json` -- `ApiKey`.

### Ejemplo rápido (curl)

```bash
curl -H "X-API-KEY: test-api-key-12345" http://localhost:5191/users
```

## Qué está implementado

- [x] Proyecto .NET 10 Minimal API que compila
- [x] Entidades `User`, `Address`, `Currency`
- [x] `AppDbContext` con EF Core + SQLite (`EnsureCreated`)
- [x] Middleware de seguridad por API Key
- [x] Swagger con soporte de API Key
- [x] Estructura de carpetas CQRS (`Application/...`)
- [x] CRUD Users (commands/queries/validators/endpoints + soft delete)
- [x] CRUD Addresses (commands/queries/validators/endpoints, FK a User)
- [x] Módulo Currencies + conversión (`POST /currency/convert`)