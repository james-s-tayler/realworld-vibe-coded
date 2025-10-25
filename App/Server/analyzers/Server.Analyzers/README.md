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
