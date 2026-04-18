---
paths:
  - "App/Server/tests/**"
---

## Functional Test Conventions

**Raw `HttpClient` methods are banned by SRV007 analyzer.** Use FastEndpoints testing extensions:

```csharp
// Preferred: fully-typed
var (response, result) = await client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(request);

// URL-based when typed approach has issues
var (response, result) = await client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?tag=test", request);
```

Use `AppFixture<Program>` for test fixtures. Only suppress SRV007 with clear justification (e.g., testing malformed JSON).
