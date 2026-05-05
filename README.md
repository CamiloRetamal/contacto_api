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

El puerto exacto aparece en consola (por defecto suele ser **`http://localhost:5233`** según `Properties/launchSettings.json`).

---

## Documentación API (Swagger)

En entorno **Development**, **Swagger UI** está habilitado para explorar y probar la API sin Postman. La UI es el documento **`swagger/index.html`**; conviene abrir la ruta completa:

**http://localhost:5233/swagger/index.html**

También suele funcionar **`http://localhost:5233/swagger`** (redirige o carga la misma UI).

Si cambias el perfil o el puerto, usa la URL base que imprima `dotnet run` y sustituye solo el host/puerto, manteniendo **`/swagger/index.html`**.

---

## Ejemplo rápido con curl

Copiar y pegar (ajusta host/puerto si tu consola muestra otro):

```bash
curl -X POST http://localhost:5233/api/contactos \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Juan","telefono":"123456"}'
```

Listado:

```bash
curl http://localhost:5233/api/contactos
```

---

## Decisiones técnicas

- **Persistencia en memoria** para acotar el alcance del desafío y evitar infraestructura externa, manteniendo el foco en la API y las reglas de negocio.
- **`lock` en el repositorio** para garantizar consistencia ante **POST concurrentes** con el mismo teléfono (comprobación de duplicado y alta atómicas).
- **CQRS ligero** (`IContactQueries` / `IContactCommands`) para separar lecturas y escrituras y facilitar tests y evolución.
- **Middleware** para correlación de peticiones y **manejo global de errores** con respuestas coherentes (`application/problem+json`).

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

## Endpoints

Ruta base: **`/api/contactos`**.

- `GET /api/contactos` — listado (puede estar vacío)
- `GET /api/contactos/{id}` — detalle o `404`
- `POST /api/contactos` — alta; `201` con cabecera `Location`, `400` si falla la validación, `409` si el teléfono ya existe

### Bonus 

- **Repositorio** — `IContactRepository` / `ContactRepository`
- **CQRS ligero** — `IContactQueries` / `ContactQueries` (lecturas) e `IContactCommands` / `ContactCommands` (escrituras)
- **Patrón Result** — registros discriminados `ContactCreateResult` para los resultados del alta (mapeados a HTTP en el controlador)
- **Records** — dominio `Contacto`, DTOs y jerarquía `ContactCreateResult`
- **Logging estructurado** — **Serilog** + `CorrelationId` en `LogContext` (ver `CorrelationIdMiddleware`, `appsettings.json`)
- **Middleware propio** — id de correlación + manejador global de excepciones (`application/problem+json`)
- **Versionado de API** — versión por defecto configurada en backend y documentación en Swagger

Ejemplo de cuerpo `POST` (nombres de propiedades JSON según el desafío):

```json
{
  "nombre": "Juan Perez",
  "telefono": "123456789"
}
```
