---
paths:
  - "App/Server/**"
---

## Endpoint Pattern (FastEndpoints)

Keep endpoints thin: bind/authorize -> delegate to MediatR command/query -> map to response.

- Use `Endpoint<TRequest, TResponse, TMapper>` with FastEndpoints mapper pattern
- Use `ResponseMapper<TResponse, TEntity>` for domain-to-DTO conversion
- Use `Map.FromEntityAsync()` in handlers to invoke mappers

## CQRS with MediatR

Business rules live in handlers. Keep them framework-agnostic where possible.

## Persistence (EF Core + SQLite)

- Explicit configurations in `EntityTypeConfiguration` classes
- `HasConversion` for value objects, unique indexes for slugs
- `AsNoTracking()` on read-only queries
- Eager loading (`Include`) to avoid N+1

## DTOs

Use `*Request`, `*Response`, `*Dto` suffixes. Only expose whitelisted fields. Map via small mappers or extension methods.

## Validation

All mutating requests must have a FluentValidation validator. Return 422 with consistent problem/details payload.

## Error Handling

Standard HTTP codes: 200/201/204, 400/401/403/404/409/422, 500. For 422 validation, return flat errors object compatible with RealWorld clients. Don't throw raw exceptions from endpoints.

**OpenAPI error declarations:** Use `ProducesProblemDetails()` (FastEndpoints), NOT `ProducesProblem()` (ASP.NET). The ASP.NET method declares standard `Microsoft.AspNetCore.Mvc.ProblemDetails` which lacks the `errors[]` array. FastEndpoints' method declares its own `ProblemDetails` type that includes `errors[]`, matching what the server actually returns. This matters for generated API clients (Kiota) — wrong type means error details are lost during deserialization.

## Logging

Structured Serilog with properties (`{@Command}`, `{UserId}`, `{Slug}`). Never log secrets or JWTs.

## Code Style

- Prefer functional composition (`Func<T>`, `Action<T>`, delegates) over OO inheritance
- Modern C# features: switch expressions, pattern matching, discards, local functions, named tuples
- Small composable functions, early returns, no deep nesting
- DRY via interfaces, generics, extension methods
- Consolidate/update existing components rather than adding new ones
