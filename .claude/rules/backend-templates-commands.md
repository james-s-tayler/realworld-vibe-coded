---
paths:
  - "App/Server/**"
---

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
