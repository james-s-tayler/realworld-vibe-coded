---
paths:
  - "App/Server/**"
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

SA1402: One type per file — always split request wrapper and data into separate files.

```csharp
// File: App/Server/src/Server.Web/{Feature}/Create/CreateRequest.cs
namespace Server.Web.{Feature}.Create;

public class CreateRequest
{
  // Wrap in an outer object to match RealWorld JSON: { "article": { ... } }
  public CreateData {Feature} { get; set; } = new();
}
```

```csharp
// File: App/Server/src/Server.Web/{Feature}/Create/CreateData.cs
namespace Server.Web.{Feature}.Create;

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
