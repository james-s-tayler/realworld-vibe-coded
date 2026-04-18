---
paths:
  - "App/Server/**"
---

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

    // Global PropertyNameResolver strips the chain to the leaf member; FastEndpoints' camelCase policy
    // then produces the RealWorld-compliant field name (e.g. "title"). Never call .OverridePropertyName() —
    // SRV022 enforces this as a build error.
    RuleFor(x => x.{Feature}.Title)
      .NotEmpty().WithMessage("is required.")
      .MaximumLength({Entity}.TitleMaxLength).WithMessage($"cannot exceed {{Entity}.TitleMaxLength} characters.");

    RuleFor(x => x.{Feature}.Description)
      .NotEmpty().WithMessage("is required.");
  }
}
```
