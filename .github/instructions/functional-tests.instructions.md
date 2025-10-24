---
applyTo: "App/Server/tests/Server.FunctionalTests/**"
---

# Functional Tests Guidelines

## Using HttpClient in Tests

**IMPORTANT:** Raw `HttpClient` methods (like `GetAsync`, `PostAsync`, `DeleteAsync`) are banned by the SRV007 analyzer. Always use FastEndpoints testing extension methods instead.

### Preferred Approach: Fully-Typed FastEndpoints Extension Methods

Use the fully-typed approach when possible for maximum type safety:

```csharp
// ✅ Good - Fully typed with endpoint type
var request = new CreateArticleRequest
{
  Article = new ArticleData
  {
    Title = "Test Article",
    Description = "Test Description",
    Body = "Test Body"
  }
};
var (response, result) = await client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(request);
```

For GET requests without parameters:
```csharp
// ✅ Good - Simple GET
var (response, result) = await client.GETAsync<ListArticles, ArticlesResponse>();
```

### When Fully-Typed Approach Has Issues

If the fully-typed approach causes unexpected behavior (e.g., BadRequest due to default property values), use the URL-based overload:

```csharp
// ✅ Acceptable - URL-based with request DTO
var request = new ListArticlesRequest();
var (response, result) = await client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?tag=test", request);
```

**Why this works:** This overload constructs the request using the explicit URL while still leveraging FastEndpoints' deserialization for the response.

### Common Pitfalls

**Problem:** Setting only some properties in a request DTO with defaults causes all properties (including defaults) to be serialized as query parameters, which may not match the original test intent.

```csharp
// ❌ May fail - Sends limit=20, offset=0 in addition to author
var request = new ListArticlesRequest { Author = "test" }; // Limit=20, Offset=0 are defaults
var (response, result) = await client.GETAsync<ListArticles, ListArticlesRequest, ArticlesResponse>(request);
```

**Solution:** Use the URL-based overload when you need precise control over query parameters:

```csharp
// ✅ Good - Only sends author parameter
var request = new ListArticlesRequest();
var (response, result) = await client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?author=test", request);
```

### Edge Cases Requiring Raw HttpClient

In rare cases, you may need raw `HttpClient` methods to test edge cases. These require suppression with clear justification:

```csharp
// Testing with invalid/malformed JSON
// SRV007: Using raw HttpClient.PostAsJsonAsync is necessary here to test deserialization error
// handling with malformed JSON. FastEndpoints POSTAsync would not allow sending invalid JSON.
#pragma warning disable SRV007
var response = await client.PostAsJsonAsync(route, "{", cancellationToken);
#pragma warning restore SRV007
```

```csharp
// Testing with invalid route parameter types
// SRV007: Using raw HttpClient.DeleteAsync is necessary here to test invalid comment ID format
// (non-numeric "abc"). FastEndpoints DELETEAsync would require a valid DeleteCommentRequest with int Id,
// which would not allow testing this edge case.
#pragma warning disable SRV007
var response = await client.DeleteAsync($"/api/articles/{slug}/comments/abc", cancellationToken);
#pragma warning restore SRV007
```

## Test Fixture Pattern

Use test fixtures for shared setup and HttpClient instances:

```csharp
public class ArticlesFixture : AppFixture<Program>
{
  public HttpClient ArticlesUser1Client { get; private set; } = null!;
  public HttpClient ArticlesUser2Client { get; private set; } = null!;

  protected override async ValueTask SetupAsync()
  {
    // Create authenticated clients
    var (user1Token, _) = await CreateUserAndLogin("user1", "user1@test.com", "password123");
    ArticlesUser1Client = CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user1Token);
    });
  }
}
```

## Best Practices

1. **Type Safety First:** Always prefer typed FastEndpoints methods over raw HttpClient
2. **Document Suppressions:** When raw HttpClient is needed, clearly explain why in comments
3. **Use Fixtures:** Share HttpClient instances and setup logic via fixtures
4. **Test Real Scenarios:** Use actual request/response DTOs that match production code
5. **Validate Responses:** Always check both status codes and response content
6. **Keep Tests Focused:** Each test should verify one behavior or scenario

## Running Tests

```bash
# Run all functional tests
./build.sh TestServer

# Run specific test class
dotnet test --filter "FullyQualifiedName~ArticlesTests"

# Check test reports
cat Reports/Server/Artifacts/Tests/Report.md
```
