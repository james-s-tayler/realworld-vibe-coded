---
paths:
  - "App/Server/**"
---

## Roslyn Analyzer Cheat Sheet — Common Pitfalls

These analyzers fire as build errors. Know them before writing code.

### PV014: ICommand handlers must call repository mutation methods
Every `ICommandHandler<TCommand, TResult>` must call at least one of:
`AddAsync`, `AddRangeAsync`, `UpdateAsync`, `UpdateRangeAsync`, `DeleteAsync`, `DeleteRangeAsync`
on an `IRepository<T>`. If your handler mutates via navigation properties (e.g., `parent.Children.Add()`),
you MUST also call `UpdateAsync(parent)` on the parent's repository to satisfy PV014.

### SRV016/SRV017: Aggregate root location
- Each `IAggregateRoot` entity must live in `Server.Core.<EntityName>Aggregate` namespace
- Only ONE `IAggregateRoot` per `*Aggregate` namespace
- If an entity needs `IRepository<T>`, it MUST extend `EntityBase, IAggregateRoot` in its own namespace

### SRV003: Send method restrictions
Only `Send.ResultMapperAsync` and `Send.ResultValueAsync` are allowed in endpoints.
Never use `Send.OkAsync`, `Send.ErrorsAsync`, `Send.NotFoundAsync`, etc.

### SRV018: No inline mappers
Always use `Endpoint<TRequest, TResponse, TMapper>` with a dedicated `ResponseMapper` class.
Never pass lambda mappers to `ResultMapperAsync`.

### SRV005: No EndpointWithoutRequest
Every endpoint needs a request type. Use `Endpoint<TRequest, TResponse>` or
`Endpoint<TRequest, TResponse, TMapper>`.

### SRV001: No non-generic Result
Use `Result<T>` always.

### SRV015: No `Result<Unit>`
Never use `Result<Unit>`. For delete/void operations, use `Result<{Entity}>` with the entity being
acted on. Return `Result<{Entity}>.NoContent()` for HTTP 204 responses.

### SA1402: One type per file (StyleCop)
Every class, record, and struct MUST be in its own file. Never put `CreateRequest` and `CreateData`
in the same file — split them into `CreateRequest.cs` and `CreateData.cs`.

### FastEndpoints: EmptyRequest for no-body endpoints
Request DTOs with zero properties throw `TypeInitializationException` at runtime.
For GET/DELETE endpoints with no request body (only route params), use `EmptyRequest` from FastEndpoints.

### FastEndpoints: `[RouteParam]` on route-only PUT/POST/PATCH endpoints
FastEndpoints defaults to requiring `Content-Type: application/json` for PUT/POST/PATCH. If the request DTO only has route parameters (no JSON body), Kiota and other clients send no body and no Content-Type, causing **415 Unsupported Media Type**. Annotate ALL route-bound properties with `[RouteParam]` — when every property has a non-JSON binding source attribute, FastEndpoints automatically accepts `*/*` (since v6.2). This is required for Kiota compatibility.
```csharp
// WRONG — returns 415 when called without Content-Type header
public class DeactivateUserRequest
{
  public Guid UserId { get; set; }
}

// CORRECT — accepts */* because all properties are annotated
public class DeactivateUserRequest
{
  [RouteParam]
  public Guid UserId { get; set; }
}
```

### Key import: `using Server.Infrastructure;`
Required in ALL web endpoint files for `ResultMapperAsync` and `ResultValueAsync` extension methods.
This is the #1 most-forgotten import — add it to every endpoint file.

## Common Gotchas

- **EF Core inverse navigation:** When adding an inverse navigation property (e.g., `Article.Favorites`), update the relationship config to `.WithMany(a => a.Favorites)` — otherwise EF creates duplicate FK columns
- **Partial update validation:** `PUT` endpoints with optional fields (e.g., Update*) must validate at least one field is provided — reject empty `{}` payloads

## Result Status -> HTTP Code Mapping

- `Result.Success()` -> 200 OK
- `Result.Created()` -> 201 Created
- `Result.NoContent()` -> 204 No Content
- `Result.Invalid()` -> 400 Bad Request (use for validation errors)
- `Result.Forbidden()` -> 403 Forbidden
- `Result.NotFound()` -> 404 Not Found
- `Result.Error()` -> 422 Unprocessable Entity (use for business rule violations)

Use `ErrorDetail(fieldName, message)` from `Server.SharedKernel.Result` for error details.
NOT `ValidationError` — that type doesn't exist.
