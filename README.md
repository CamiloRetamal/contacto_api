# API de contactos (.NET)

API REST en **.NET 8** para gestionar contactos **sin base de datos** (persistencia en memoria en el mismo proceso), con capas claras (controladores, servicios, repositorio, middleware).

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Ejecución

Desde la raíz de este proyecto:

```bash
dotnet run --project src/Contactos.Api/Contactos.Api.csproj
```

La API escucha en `http://localhost:5xxx` (el puerto exacto aparece en consola). En entorno **Development**, **Swagger** está disponible en `/swagger`.

---

## Tests

```bash
dotnet test
```

Incluye tests **unitarios** (servicio / reglas) y tests de **integración HTTP** con `WebApplicationFactory`.

### Concurrencia (requisito del desafío)

El repositorio mantiene el estado compartido bajo un único **`lock`**: la comprobación de duplicado y el alta en `POST` ocurren en **una** llamada a `Add`, de modo que peticiones paralelas con el mismo teléfono no pueden dar ambas de alta. Un test de integración (`Concurrent_posts_same_telephone_…`) lanza **50** `POST` concurrentes con el mismo número y comprueba que haya **exactamente un `201 Created`** y el resto **`409 Conflict`**.

---

## Arquitectura (resumen)

| Capa              | Rol                                                                                   |
| ----------------- | ------------------------------------------------------------------------------------- |
| `Controllers`     | HTTP, códigos de estado, contratos de la API                                          |
| `Services`        | Reglas de negocio (validación, duplicados)                                            |
| `Repositories`    | Persistencia en proceso, **segura entre hilos** (`lock`)                              |
| `Core/Middleware` | Correlación de peticiones y manejo global de excepciones (`application/problem+json`) |

---

## Endpoints (versionados)

Ruta base: **`/api/v1/contactos`** (versionado por URL con `Asp.Versioning`).

- `GET /api/v1/contactos` — listado (puede estar vacío)
- `GET /api/v1/contactos/{id}` — detalle o `404`
- `POST /api/v1/contactos` — alta; `201` con cabecera `Location`, `400` si falla la validación, `409` si el teléfono ya existe

### Bonus 

- **Repositorio** — `IContactRepository` / `ContactRepository`
- **CQRS ligero** — `IContactQueries` / `ContactQueries` (lecturas) e `IContactCommands` / `ContactCommands` (escrituras)
- **Patrón Result** — registros discriminados `ContactCreateResult` para los resultados del alta (mapeados a HTTP en el controlador)
- **Records** — dominio `Contacto`, DTOs y jerarquía `ContactCreateResult`
- **Logging estructurado** — **Serilog** + `CorrelationId` en `LogContext` (ver `CorrelationIdMiddleware`, `appsettings.json`)
- **Middleware propio** — id de correlación + manejador global de excepciones (`application/problem+json`)
- **Versionado de API** — v1 en la URL; Swagger UI muestra el documento versionado

Ejemplo de cuerpo `POST` (nombres de propiedades JSON según el desafío):

```json
{
  "nombre": "Juan Perez",
  "telefono": "123456789"
}
```
