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
Use `Result<T>` always. For void operations: `Result<Unit>`.

### Key import: `using Server.Infrastructure;`
Required in ALL web endpoint files for `ResultMapperAsync` and `ResultValueAsync` extension methods.
This is the #1 most-forgotten import — add it to every endpoint file.

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

---

## Code Templates

Copy-paste these templates when creating new features. Customize the marked sections.

### Endpoint (authenticated POST)

```csharp
// File: App/Server/src/Server.Web/{Feature}/Create/Create.cs
// REQUIRED: This import provides ResultMapperAsync/ResultValueAsync extension methods
using Server.Infrastructure;
using Server.UseCases.{Feature};
using Server.UseCases.{Feature}.Create;
using Server.UseCases.Interfaces;

namespace Server.Web.{Feature}.Create;

public class Create(IMediator mediator, IUserContext userContext) : Endpoint<CreateRequest, {Feature}Response, {Feature}Mapper>
{
  public override void Configure()
  {
    Post("/api/{route}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(CreateRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new CreateCommand(/* map request fields */, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (entity, ct) => await Map.FromEntityAsync(entity, ct),
      cancellationToken);
  }
}
```

### Request + Response DTOs

```csharp
// File: App/Server/src/Server.Web/{Feature}/Create/CreateRequest.cs
namespace Server.Web.{Feature}.Create;

public class CreateRequest
{
  // Wrap in an outer object to match RealWorld JSON: { "article": { ... } }
  public CreateData {Feature} { get; set; } = new();
}

public class CreateData
{
  public string Title { get; set; } = default!;
  public string Description { get; set; } = default!;
  public string Body { get; set; } = default!;
  public List<string>? TagList { get; set; }
}
```

```csharp
// File: App/Server/src/Server.Web/{Feature}/{Feature}Response.cs
namespace Server.Web.{Feature};

public class {Feature}Response
{
  // Match the exact shape from SPEC-REFERENCE.md
  public {Feature}Dto {Feature} { get; set; } = default!;
}
```

### MediatR Command + Handler

```csharp
// File: App/Server/src/Server.UseCases/{Feature}/Create/CreateCommand.cs
using Server.SharedKernel.MediatR;

namespace Server.UseCases.{Feature}.Create;

public record CreateCommand(
  string Title,
  string Description,
  string Body,
  List<string> TagList,
  Guid AuthorId,
  Guid UserId
) : ICommand<{Entity}>;
```

```csharp
// File: App/Server/src/Server.UseCases/{Feature}/Create/CreateHandler.cs
using Microsoft.Extensions.Logging;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.{Feature}.Create;

public class CreateHandler : ICommandHandler<CreateCommand, {Entity}>
{
  private readonly IRepository<{Entity}> _repository;
  private readonly ILogger<CreateHandler> _logger;

  public CreateHandler(
    IRepository<{Entity}> repository,
    ILogger<CreateHandler> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  public async Task<Result<{Entity}>> Handle(CreateCommand request, CancellationToken cancellationToken)
  {
    // Create entity from command
    var entity = new {Entity}(/* ... */);

    // PV014: Handler MUST call a mutation method (AddAsync/UpdateAsync/DeleteAsync) on IRepository<T>
    await _repository.AddAsync(entity, cancellationToken);

    _logger.LogInformation("Created {Entity} with ID {Id}", entity.Id);

    return Result<{Entity}>.Success(entity);
  }
}
```

### MediatR Query + Handler

```csharp
// File: App/Server/src/Server.UseCases/{Feature}/Get/GetQuery.cs
using Server.SharedKernel.MediatR;

namespace Server.UseCases.{Feature}.Get;

public record GetQuery(string Identifier) : IQuery<{ResultDto}>;
```

```csharp
// File: App/Server/src/Server.UseCases/{Feature}/Get/GetHandler.cs
using Server.SharedKernel.MediatR;

namespace Server.UseCases.{Feature}.Get;

public class GetHandler : IQueryHandler<GetQuery, {ResultDto}>
{
  private readonly IReadRepository<{Entity}> _repository;

  public GetHandler(IReadRepository<{Entity}> repository)
  {
    _repository = repository;
  }

  public async Task<Result<{ResultDto}>> Handle(GetQuery request, CancellationToken cancellationToken)
  {
    var spec = new {Entity}ByIdentifierSpec(request.Identifier);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (entity is null)
      return Result<{ResultDto}>.NotFound();

    return Result<{ResultDto}>.Success(/* map to DTO */);
  }
}
```

### FluentValidation Validator

```csharp
// File: App/Server/src/Server.Web/{Feature}/Create/CreateValidator.cs
using FluentValidation;

namespace Server.Web.{Feature}.Create;

public class CreateValidator : Validator<CreateRequest>
{
  public CreateValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.{Feature}.Title)
      .NotEmpty().WithMessage("is required.")
      .MaximumLength({Entity}.TitleMaxLength).WithMessage($"cannot exceed {{Entity}.TitleMaxLength} characters.")
      .OverridePropertyName("title");

    RuleFor(x => x.{Feature}.Description)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("description");
  }
}
```

### EF Core Entity Configuration

```csharp
// File: App/Server/src/Server.Infrastructure/Data/Config/{Entity}Configuration.cs
using Server.Core.{Aggregate};

namespace Server.Infrastructure.Data.Config;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
  public void Configure(EntityTypeBuilder<{Entity}> builder)
  {
    // Property constraints
    builder.Property(x => x.Title)
      .HasMaxLength({Entity}.TitleMaxLength)
      .IsRequired();

    builder.Property(x => x.Slug)
      .HasMaxLength({Entity}.SlugMaxLength)
      .IsRequired();

    // Unique indexes
    builder.HasIndex(x => x.Slug).IsUnique();

    // Relationships
    builder.HasOne(x => x.Author)
      .WithMany()
      .HasForeignKey(x => x.AuthorId)
      .OnDelete(DeleteBehavior.Restrict);

    // Many-to-many
    builder.HasMany(x => x.Tags)
      .WithMany(x => x.Articles)
      .UsingEntity(j => j.ToTable("ArticleTags"));
  }
}
```

### ResponseMapper

```csharp
// File: App/Server/src/Server.Web/{Feature}/{Feature}Mapper.cs
using Server.Core.{Aggregate};

namespace Server.Web.{Feature};

public class {Feature}Mapper : ResponseMapper<{Feature}Response, {Entity}>
{
  public override Task<{Feature}Response> FromEntityAsync({Entity} entity, CancellationToken ct)
  {
    var response = new {Feature}Response
    {
      {Feature} = new {Feature}Dto
      {
        // Map only the fields SPEC-REFERENCE.md requires
        Title = entity.Title,
        Slug = entity.Slug,
        Description = entity.Description,
        Body = entity.Body,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
      },
    };

    return Task.FromResult(response);
  }
}
```
