---
applyTo: "App/Server/**"
---

# Conduit backend Guidelines

## Conventions & Structure

* **Project layout:** Follow existing folders. When adding new features, prefer `Endpoints/<Feature>/` for transport code and `Application/<Feature>/` for MediatR + validators. Keep domain entities/aggregates separate from DTOs.
* **DTOs:** Use `*Request`, `*Response`, `*Dto` suffixes. Only expose whitelisted fields. Map via small mappers or extension methods.
* **Validation:** All mutating requests must have a FluentValidation validator. Return 422 with a consistent problem/details payload on validation failures.
* **Logging:** Use Serilog with structured properties (e.g., `{@Command}`, `{UserId}`, `{Slug}`).
* **Pagination:** For list endpoints, support `limit`, `offset` (defaults e.g., `limit=20`, `offset=0`). Cap `limit` to a safe max (e.g., 100).
* **Transactions:** For multi-write operations, use EF Core transactions or `SaveChanges` boundaries that ensure atomicity.
* **Time:** Store timestamps in UTC; convert at edges if needed.

## Endpoint Pattern (FastEndpoints)

Keep endpoints **thin**: bind/authorize → delegate to a MediatR command/query → map to response.

### FastEndpoints Mappers

FastEndpoints provides a first-class mapper pattern for converting domain entities to response DTOs. Use `ResponseMapper<TResponse, TEntity>` to define mappers:

```csharp
/// <summary>
/// FastEndpoints mapper for ApplicationUser to UsersResponse DTO
/// </summary>
public class UserMapper : ResponseMapper<UsersResponse, List<ApplicationUser>>
{
  public override Task<UsersResponse> FromEntityAsync(List<ApplicationUser> users, CancellationToken ct)
  {
    var userDtos = users.Select(user => new UserDto
    {
      Email = user.Email!,
      Username = user.UserName!,
      Bio = user.Bio,
      Image = user.Image,
    }).ToList();

    return Task.FromResult(new UsersResponse { Users = userDtos });
  }
}
```

**Using mappers in endpoints:**
1. Specify the mapper as the third type parameter: `Endpoint<TRequest, TResponse, TMapper>`
2. Use `Map.FromEntityAsync()` in the handler to invoke the mapper
3. Mappers can resolve dependencies via `Resolve<T>()` for complex mapping logic (e.g., current user context)

Example endpoint:
```csharp
public class ListUsers(IMediator mediator) : Endpoint<EmptyRequest, UsersResponse, UserMapper>
{
  public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
  {
    var result = await mediator.Send(new ListUsersQuery(), ct);
    await Send.ResultMapperAsync(result, async (users, ct) => await Map.FromEntityAsync(users, ct), ct);
  }
}
```

## CQRS with MediatR

Business rules live in handlers. Keep them framework-agnostic where possible.

## Persistence (EF Core + SQL Server)

* Prefer explicit configurations in `DbContext`/`EntityTypeConfiguration` classes (keys, indexes, required fields, max lengths, relationships, cascade behavior).
* Use `HasConversion` for value objects. Keep slugs unique with a unique index.
* Use `AsNoTracking()` on read-only queries.

## Errors & Results

* Use standard HTTP codes: `200/201/204`, `400/401/403/404/409/422`, `500`.
* For 422 validation issues, return a flat errors object compatible with RealWorld/Conduit clients.
* Do not throw raw exceptions from endpoints; map to problem responses centrally.

## Testing Expectations

* Prefer integration tests covering the full stack (endpoint → MediatR → EF Core).
* Use xUnit; follow AAA pattern (Arrange, Act, Assert).
* Tests run against a SQL Server instance managed by `docker-compose.dev-deps.yml` which is started automatically by the Nuke build system.

## Performance & Security

* Use eager loading (`Include`) to avoid N+1 queries.
* Never log secrets or JWTs.

## When Copilot makes changes

1. Update DTOs, validators, handler, and EF mappings together.
2. Add/adjust tests in the same PR; ensure `dotnet test` passes.

---

**Scope:** Backend only. Do not modify frontend or infra from instructions in this file.

