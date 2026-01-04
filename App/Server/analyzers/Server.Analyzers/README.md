# Server.Analyzers

This project contains custom Roslyn analyzers that enforce best practices and coding standards for the Server project.

## Analyzers

### SRV001: NonGenericResultAnalyzer
**Description:** Enforces the use of generic `Result<T>` instead of non-generic `Result`.

**Severity:** Error

**Rationale:** Non-generic Result is deprecated in Ardalis.Result v10+. The generic version provides better type safety and clarity.

**Fix:** Replace `Result` with `Result<T>`. For operations without a return value, use `Result<Unit>`.

**Example:**
```csharp
// ❌ Bad
public Task<Result> DoSomething();

// ✅ Good
public Task<Result<Unit>> DoSomething();
public Task<Result<MyData>> GetData();
```

---

### SRV002: EmptyMapperAnalyzer
**Description:** Detects empty mapper classes that don't provide any value.

**Severity:** Warning

**Rationale:** Mappers should contain actual mapping logic. Empty mappers add unnecessary complexity.

---

### SRV003: SendMethodUsageAnalyzer
**Description:** Restricts the usage of FastEndpoints Send methods in endpoint classes to only `ResultValueAsync` and `ResultMapperAsync`.

**Severity:** Error

**Rationale:** Direct usage of FastEndpoints Send methods (like `Send.OkAsync`, `Send.ErrorsAsync`, etc.) bypasses the standardized Result pattern handling, which provides consistent error handling, status code mapping, and validation error formatting.

**Allowed methods:**
- `Send.ResultValueAsync`
- `Send.ResultMapperAsync`

**Example:**
```csharp
// ❌ Bad
await Send.OkAsync(data, cancellationToken);
await Send.ErrorsAsync(errors, cancellationToken);

// ✅ Good
await Send.ResultValueAsync(result, cancellationToken);
await Send.ResultMapperAsync(result, mapper, cancellationToken);
```

---

### SRV004-SRV006: Other Analyzers
Reserved for internal analyzers.

---

### SRV018: InlineMapperInResultMapperAsyncAnalyzer
**Description:** Enforces the use of dedicated FastEndpoints.ResponseMapper classes instead of inline lambda mappers with `Send.ResultMapperAsync`.

**Severity:** Error

**Rationale:** Endpoints using `Send.ResultMapperAsync` with inline lambda mappers should be refactored to use the three-parameter `Endpoint<TRequest, TResponse, TMapper>` pattern with a dedicated `ResponseMapper` class. This ensures consistent mapper architecture, enables mapper reusability, and maintains separation of concerns.

**Fix:** Define the endpoint with three type parameters `Endpoint<TRequest, TResponse, TMapper>` where `TMapper` is a `FastEndpoints.ResponseMapper<TResponse, TEntity>` class, then use `Map.FromEntityAsync()` in the endpoint handler.

**Example:**
```csharp
// ❌ Bad - Inline mapper with ResultMapperAsync
public class GetCurrent : Endpoint<EmptyRequest, UserCurrentResponse>
{
  public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
  {
    var result = await mediator.Send(query, ct);
    await Send.ResultMapperAsync(
      result,
      user => new UserCurrentResponse
      {
        User = new UserResponse
        {
          Email = user.Email!,
          Username = user.UserName!,
          Bio = user.Bio ?? string.Empty,
          Image = user.Image,
        },
      },
      ct);
  }
}

// ✅ Good - Dedicated mapper class with three-parameter Endpoint
public class GetCurrent : Endpoint<EmptyRequest, UserCurrentResponse, UserMapper>
{
  public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
  {
    var result = await mediator.Send(query, ct);
    await Send.ResultMapperAsync(result, async (user, ct) => await Map.FromEntityAsync(user, ct), ct);
  }
}

public class UserMapper : ResponseMapper<UserCurrentResponse, ApplicationUser>
{
  public override Task<UserCurrentResponse> FromEntityAsync(ApplicationUser user, CancellationToken ct)
  {
    var response = new UserCurrentResponse
    {
      User = new UserResponse
      {
        Email = user.Email!,
        Username = user.UserName!,
        Bio = user.Bio ?? string.Empty,
        Image = user.Image,
      },
    };
    return Task.FromResult(response);
  }
}
```

---

### SRV009: ResponseMapperGetAwaiterAnalyzer
**Description:** Bans usage of `GetAwaiter().GetResult()` in FastEndpoints.ResponseMapper classes.

**Severity:** Error

**Rationale:** Using `GetAwaiter().GetResult()` within FastEndpoints.ResponseMapper classes can cause deadlocks and poor asynchronous behavior. ResponseMapper classes should use asynchronous patterns properly.

**Fix:** Override `FromEntityAsync` instead of `FromEntity` and use `await` for asynchronous calls.

**Example:**
```csharp
// ❌ Bad - Using GetAwaiter().GetResult() in FromEntity
public class ArticleMapper : ResponseMapper<ArticleResponse, Article>
{
  public override ArticleResponse FromEntity(Article article)
  {
    var userRepository = Resolve<IRepository<User>>();
    var currentUser = userRepository.FirstOrDefaultAsync(spec).GetAwaiter().GetResult();
    // ... rest of mapping
  }
}

// ✅ Good - Using await in FromEntityAsync
public class ArticleMapper : ResponseMapper<ArticleResponse, Article>
{
  public override async Task<ArticleResponse> FromEntityAsync(Article article, CancellationToken ct)
  {
    var userRepository = Resolve<IRepository<User>>();
    var currentUser = await userRepository.FirstOrDefaultAsync(spec, ct);
    // ... rest of mapping
  }
}
```

---

### SRV007: BanRawHttpClientAnalyzer
**Description:** Bans raw usage of `HttpClient` methods in functional tests, encouraging the use of FastEndpoints testing extension methods.

**Severity:** Error

**Scope:** Server.FunctionalTests project only

**Rationale:** Raw HttpClient methods like `GetAsync`, `PostAsync`, `DeleteAsync` should be avoided in functional tests. FastEndpoints provides type-safe extension methods (`POSTAsync`, `GETAsync`, `DELETEAsync`, etc.) that offer:
- Better type safety with strongly-typed request/response DTOs
- Automatic JSON serialization/deserialization
- More readable and maintainable test code
- Consistent testing patterns across the codebase

**Allowed:** HttpClient as properties or fields in test fixtures (for dependency injection from WebApplicationFactory).

**Banned methods:**
- `HttpClient.SendAsync`
- `HttpClient.GetAsync`
- `HttpClient.PostAsync`
- `HttpClient.PutAsync`
- `HttpClient.DeleteAsync`
- `HttpClient.PatchAsync`
- `HttpClient.GetStringAsync`
- `HttpClient.GetByteArrayAsync`
- `HttpClient.GetStreamAsync`

**Preferred alternatives:**
```csharp
// ❌ Bad - Raw HttpClient
var response = await client.GetAsync("/api/articles?tag=test");
var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();

// ✅ Good - FastEndpoints extension
var request = new ListArticlesRequest { Tag = "test" };
var (response, result) = await client.GETAsync<ListArticles, ListArticlesRequest, ArticlesResponse>(request);
```

**Exception cases (requires suppression with justification):**
- Testing invalid/malformed requests (e.g., invalid JSON for deserialization error tests)
- Testing edge cases where type safety would prevent the test scenario
- Testing with invalid URL parameters that don't match the typed request model

**Suppression example:**
```csharp
// SRV007: Using raw HttpClient.PostAsJsonAsync is necessary here to test deserialization error
// handling with malformed JSON. FastEndpoints POSTAsync would not allow sending invalid JSON.
#pragma warning disable SRV007
var response = await client.PostAsJsonAsync(route, "{", cancellationToken);
#pragma warning restore SRV007
```

---

### SRV012-SRV013: DevOnlyGroupAnalyzer
**Description:** Enforces FastEndpoints grouping rules for the Server.Web.DevOnly namespace.

**Severity:** Error

**Rationale:** Development-only endpoints in the Server.Web.DevOnly namespace must be explicitly grouped under `DevOnly` or a `SubGroup<DevOnly>` to ensure they are properly filtered and organized. Conversely, only endpoints in the Server.Web.DevOnly namespace should use these groups to maintain clear separation between dev-only and production code.

**Two-way enforcement:**

1. **SRV012:** All endpoints in the `Server.Web.DevOnly` namespace must call `Group<DevOnly>()` or `Group<T>()` where `T` inherits from `SubGroup<DevOnly>` in their `Configure()` method.

2. **SRV013:** Any endpoint calling `Group<DevOnly>()` or `Group<T>()` where `T` inherits from `SubGroup<DevOnly>` must be located in the `Server.Web.DevOnly` namespace.

**Example:**
```csharp
namespace Server.Web.DevOnly.Endpoints;

// ✅ Good - DevOnly endpoint with DevOnly group
public class TestEndpoint : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/dev-only/test");
    Group<DevOnly>(); // or Group<TestError>() where TestError : SubGroup<DevOnly>
  }
}

// ❌ Bad - DevOnly endpoint without DevOnly group
public class TestEndpoint : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/dev-only/test");
    // Missing Group<DevOnly>() call - SRV012 violation
  }
}

namespace Server.Web.Articles;

// ❌ Bad - Production endpoint using DevOnly group
public class GetArticle : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    Group<DevOnly>(); // SRV013 violation - wrong namespace
  }
}
```

---

### SRV010: BanXunitAssertAnalyzer
**Description:** Bans usage of `xunit.Assert` in test code, enforcing the use of Shouldly assertion methods instead.

**Severity:** Error

**Scope:** All test projects (Server.UnitTests, Server.IntegrationTests, Server.FunctionalTests, Server.SharedKernel.Result.UnitTests)

**Rationale:** Direct usage of `xunit.Assert` methods should be avoided in test code. Shouldly provides more readable and expressive assertion methods that:
- Produce better error messages that clearly show what was expected vs. what was actual
- Improve test readability with natural language syntax
- Make tests more maintainable and easier to understand
- Align with the existing test code patterns in the codebase

**Banned usage:**
- Any method call on `Xunit.Assert` class

**Preferred alternatives:**
```csharp
// ❌ Bad - xunit.Assert
Assert.Equal(expected, actual);
Assert.NotNull(result);
Assert.True(condition);
Assert.Empty(collection);

// ✅ Good - Shouldly
actual.ShouldBe(expected);
result.ShouldNotBeNull();
condition.ShouldBeTrue();
collection.ShouldBeEmpty();
```

---

### SRV011: BanNewDateTimeAnalyzer
**Description:** Bans direct instantiation of `DateTime` objects using the `new` keyword in test code.

**Severity:** Error

**Scope:** All test projects (Server.UnitTests, Server.IntegrationTests, Server.FunctionalTests, Server.SharedKernel.Result.UnitTests)

**Rationale:** Direct instantiation of `DateTime` objects using the `new` keyword should be avoided in test code. Using `DateTime.Parse` with human-readable date strings:
- Makes tests more readable and self-documenting
- Improves test maintainability
- Makes it easier to understand test data at a glance
- Reduces cognitive load when reading tests

**Banned usage:**
- `new DateTime(year, month, day)`
- `new DateTime(year, month, day, hour, minute, second)`
- Any other `DateTime` constructor invocation

**Preferred alternatives:**
```csharp
// ❌ Bad - new DateTime()
var date = new DateTime(2023, 1, 15);
var dateTime = new DateTime(2023, 1, 15, 10, 30, 0);

// ✅ Good - DateTime.Parse
var date = DateTime.Parse("2023-01-15");
var dateTime = DateTime.Parse("2023-01-15 10:30:00");
```

---

## Persistence Analyzers (PV001-PV060)

### PV001: EfCoreTypesOnlyInInfrastructureAnalyzer
**Description:** Prevents EF Core types from being referenced outside Infrastructure layer.

**Severity:** Error

**Rationale:** EF Core types (DbContext, DbSet, IEntityTypeConfiguration, Migrations) should be isolated to Infrastructure to maintain clean architecture boundaries.

**Fix:** Wrap with repository/abstraction, move code into Infrastructure, or inject an abstraction.

---

### PV002: DbContextNotInApplicationDomainAnalyzer
**Description:** Prevents DbContext from appearing in the public surface of Application/Domain layers.

**Severity:** Error

**Rationale:** DbContext should be isolated to Infrastructure layer behind abstractions like repositories or unit-of-work.

**Fix:** Use repository or unit-of-work abstractions instead of direct DbContext usage.

---

### PV003: RawSqlRestrictedToGatewaysAnalyzer
**Description:** Restricts raw SQL API usage to designated gateway classes.

**Severity:** Warning

**Rationale:** Centralizes and audits FromSqlRaw/Interpolated and ExecuteSql* usage for security and maintainability.

**Fix:** Move to a gateway class marked with [SqlGateway] or in .Data.Queries namespace.

---

### PV010: RepositoriesNoIQueryableAnalyzer
**Description:** Detects repository interfaces or classes exposing IQueryable or EF types.

**Severity:** Error

**Rationale:** Avoid leaking query providers and EF-specific concerns across boundaries.

**Fix:** Return IEnumerable, IAsyncEnumerable, or concrete domain types instead.

---

### PV011: RepositoryAsyncMethodsCancellationTokenAnalyzer
**Description:** Ensures repository async methods support CancellationToken.

**Severity:** Warning

**Rationale:** Cooperative cancellation is critical for I/O-bound operations.

**Fix:** Add CancellationToken parameter and pass to EF async methods.

---

### PV012: SaveChangesOnlyInUnitOfWorkAnalyzer
**Description:** Ensures SaveChanges/SaveChangesAsync is only called in UnitOfWork implementations.

**Severity:** Error

**Rationale:** Centralizes transaction management and domain event dispatch.

**Fix:** Use IUnitOfWork.CommitAsync() instead of direct SaveChanges calls.

---

### PV013: DomainEntitiesNoEfAttributesAnalyzer
**Description:** Detects EF mapping attributes on domain entities.

**Severity:** Error

**Rationale:** Keep domain persistence-agnostic.

**Fix:** Move mapping to Fluent API in IEntityTypeConfiguration<T>.

**Example:**
```csharp
// ❌ Bad - Domain entity with EF attributes
[Table("Users")]
public class User
{
  [Key]
  public int Id { get; set; }
}

// ✅ Good - Clean domain entity
public class User
{
  public int Id { get; set; }
}

// Configuration in Infrastructure
public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");
    builder.HasKey(u => u.Id);
  }
}
```

---

### PV020: PreferAsNoTrackingForReadOnlyQueriesAnalyzer
**Description:** Suggests using AsNoTracking for read-only queries.

**Severity:** Info

**Rationale:** Reduce change-tracking overhead for reads.

**Fix:** Insert .AsNoTracking() before materialization methods.

---

### PV021: AvoidMaterializationBeforeProjectionAnalyzer
**Description:** Detects materialization (ToList/AsEnumerable) before projection or filtering.

**Severity:** Warning

**Rationale:** Push computation to the database for better performance.

**Fix:** Reorder to perform Select/Where before ToList/ToArray.

---

### PV022: UseAsyncEfVariantsAnalyzer
**Description:** Detects synchronous EF methods being used in async methods.

**Severity:** Warning

**Rationale:** Avoid sync blocking on I/O operations.

**Fix:** Use ToListAsync, FirstAsync, etc. instead of synchronous variants.

---

### PV023: BlockOnAsyncEfCallsAnalyzer
**Description:** Detects blocking on async Entity Framework calls using .Result or .Wait().

**Severity:** Error

**Rationale:** Prevent deadlocks and thread pool starvation.

**Fix:** Use await with CancellationToken.

---

### PV030: ForbidDateTimeNowInQueriesAnalyzer
**Description:** Forbids DateTime.Now and DateTimeOffset.Now in LINQ queries.

**Severity:** Error

**Rationale:** Timezone consistency and proper database translation.

**Fix:** Use DateTime.UtcNow or DateTimeOffset.UtcNow.

---

### PV032: BanStringBasedIncludeAnalyzer
**Description:** Bans string-based Include calls in Entity Framework queries.

**Severity:** Error

**Rationale:** Avoid brittle magic strings.

**Fix:** Use strongly typed Include/ThenInclude with lambda expressions.

**Example:**
```csharp
// ❌ Bad
query.Include("Author").Include("Tags")

// ✅ Good
query.Include(a => a.Author).ThenInclude(a => a.Tags)
```

---

### PV040: EntityConfigurationLocationAnalyzer
**Description:** Ensures IEntityTypeConfiguration implementations are in the correct namespace.

**Severity:** Warning

**Rationale:** Keep EF mapping centralized and discoverable.

**Fix:** Move to .Data.Config or .Infrastructure.Data.Config namespace.

---

### PV041: NoMigrationReferencesInApplicationDomainAnalyzer
**Description:** Prevents migration classes from being referenced outside Infrastructure layer.

**Severity:** Error

**Rationale:** Migrations are implementation details.

**Fix:** Remove references or move logic to Infrastructure.

---

### PV042: NoEnsureCreatedInProductionAnalyzer
**Description:** Prevents usage of EnsureCreated/EnsureDeleted in production code.

**Severity:** Error

**Rationale:** Avoid destructive or schema-bypassing APIs at runtime.

**Fix:** Use EF Core migrations or guard behind environment checks.

---

### PV050: PublicApisNoEfEntitiesAnalyzer
**Description:** Prevents public APIs from exposing EF entities directly.

**Severity:** Error

**Rationale:** Prevent coupling of transport and persistence models.

**Fix:** Map to DTOs or domain models.

---

### PV051: NoInfrastructureTypesInApplicationDomainAnalyzer
**Description:** Prevents Infrastructure types from being injected into Application/Domain layers.

**Severity:** Error

**Rationale:** Depend on abstractions, not concrete implementations.

**Fix:** Inject IUnitOfWork or repository interfaces instead.

---

## Domain Design Analyzers (SRV016-SRV020)

### SRV016: AggregateRootLocationAnalyzer - Wrong Location
**Description:** Enforces that entities implementing `IAggregateRoot` must be located in a matching `*Aggregate` namespace.

**Severity:** Error

**Rationale:** Maintains clear Domain-Driven Design aggregate boundaries by ensuring each aggregate root resides in its own dedicated namespace/directory.

**Rule:** An entity implementing `IAggregateRoot` must be in namespace `Server.Core.<EntityName>Aggregate` where `EntityName` matches the entity class name.

**Fix:** Move the aggregate root to the correct `*Aggregate` directory and update the namespace.

**Example:**
```csharp
// ❌ Bad - Tag aggregate root in ArticleAggregate namespace
namespace Server.Core.ArticleAggregate;
public class Tag : EntityBase, IAggregateRoot { }

// ✅ Good - Tag aggregate root in its own TagAggregate namespace
namespace Server.Core.TagAggregate;
public class Tag : EntityBase, IAggregateRoot { }
```

---

### SRV017: AggregateRootLocationAnalyzer - Multiple Roots
**Description:** Enforces that each `*Aggregate` namespace contains only one entity implementing `IAggregateRoot`.

**Severity:** Error

**Rationale:** Maintains clear aggregate boundaries in Domain-Driven Design. Each aggregate should have exactly one root entity that controls access to the aggregate's internals.

**Rule:** Only one entity per `*Aggregate` namespace can implement `IAggregateRoot`.

**Fix:** If multiple entities in the same aggregate namespace implement `IAggregateRoot`, either:
- Move one of them to its own `*Aggregate` directory (if it should be a separate aggregate)
- Remove the `IAggregateRoot` interface from child entities (if they should be part of the existing aggregate)

**Example:**
```csharp
// ❌ Bad - Both Article and Tag implement IAggregateRoot in ArticleAggregate
namespace Server.Core.ArticleAggregate;
public class Article : EntityBase, IAggregateRoot { }
public class Tag : EntityBase, IAggregateRoot { }  // Violation!

// ✅ Good - Each aggregate root in its own namespace
namespace Server.Core.ArticleAggregate;
public class Article : EntityBase, IAggregateRoot { }

namespace Server.Core.TagAggregate;
public class Tag : EntityBase, IAggregateRoot { }
```

---

## Adding a New Analyzer

To add a new analyzer to this project:

1. Create a new class that inherits from `DiagnosticAnalyzer`
2. Decorate the class with `[DiagnosticAnalyzer(LanguageNames.CSharp)]`
3. Assign a unique diagnostic ID (e.g., SRV008 for the next analyzer)
4. Define a `DiagnosticDescriptor` with appropriate severity and messages
5. Override `Initialize` and register syntax/semantic analysis actions
6. Implement your analysis logic
7. Update this README with documentation for the new analyzer
8. Add unit tests to verify the analyzer behavior

**Example structure:**
```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class MyNewAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV008";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Title",
        "Message",
        "Category",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Description");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      
      // Register your analysis actions here
    }
  }
}
```

## Testing Analyzers

Automated unit tests for analyzers are planned for future implementation. Currently, analyzer behavior is verified through:
- Manual testing during development
- Integration with the build process to catch violations in real code
- Testing the analyzer on the actual codebase (Server.FunctionalTests)

To manually test an analyzer:
1. Make changes to test code
2. Build the project
3. Verify that violations are properly detected or suppressed

Future enhancement: Add unit tests using Microsoft.CodeAnalysis.Testing framework once compatibility with .NET 9 is resolved.

## Integration

To use these analyzers in a project, add a project reference with special attributes:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\analyzers\Server.Analyzers\Server.Analyzers.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

The `OutputItemType="Analyzer"` tells MSBuild that this is an analyzer project, and `ReferenceOutputAssembly="false"` ensures the analyzer DLL is not included in the output.
