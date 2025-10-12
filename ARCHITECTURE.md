# Architecture Documentation

## Mapping Conventions

### Overview
The application uses a hybrid approach for mapping between domain entities and DTOs, following clean architecture principles and FastEndpoints best practices.

### FastEndpoints Mappers (Preferred for Single-Entity Operations)

For single-entity operations (Get, Create, Update, Favorite, etc.), we use FastEndpoints ResponseMapper classes:

**Location:** `Server.Web.Articles.ArticleMapper`

**Pattern:**
- MediatR handlers return domain entities (e.g., `Result<Article>`)
- FastEndpoints endpoints use `ResponseMapper<TResponse, TEntity>` to map entities to response DTOs
- Mapping occurs at the endpoint layer using `Map.FromEntity(entity)`

**Benefits:**
- Clear separation: Domain logic returns entities, presentation layer handles DTO mapping
- FastEndpoints integration: Built-in mapper support with dependency injection
- User context: Mappers can resolve scoped services (e.g., `ICurrentUserService`) for user-specific data

**Example:**
```csharp
// Handler returns entity
public class GetArticleHandler : IQueryHandler<GetArticleQuery, Result<Article>>
{
  public async Task<Result<Article>> Handle(...)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(...);
    return Result.Success(article);
  }
}

// Endpoint uses FastEndpoints mapper
public class Get : EndpointWithoutRequest<ArticleResponse, ArticleMapper>
{
  public override async Task HandleAsync(...)
  {
    var result = await _mediator.Send(new GetArticleQuery(...));
    if (result.IsSuccess)
    {
      Response = Map.FromEntity(result.Value);
    }
  }
}

// Mapper handles DTO conversion
public class ArticleMapper : ResponseMapper<ArticleResponse, Article>
{
  public override ArticleResponse FromEntity(Article article)
  {
    var currentUserService = Resolve<ICurrentUserService>();
    // ... mapping logic with user context
  }
}
```

### Static Utility Methods

The UseCases project contains utility methods for domain operations:
- `Server.UseCases.Articles.ArticleMappers.GenerateSlug()` - URL-friendly slug generation
- `Server.UseCases.Articles.Comments.CommentMappers` - Comment mapping (legacy)
- `Server.UseCases.Users.UserMappers` - User mapping (legacy)
- `Server.UseCases.Contributors.ContributorMappers` - Contributor mapping (legacy)

### Infrastructure Query Services (For List Operations)

For list/collection operations, Infrastructure returns entity collections and endpoints use FastEndpoints mappers:

**Pattern:**
- Infrastructure returns `IEnumerable<Article>` entities
- Handlers return `ArticlesEntitiesResult` containing entities
- Endpoints map each entity using `ArticleMapper.FromEntity()` in a loop

**Example:**
```csharp
// Infrastructure returns entities
public async Task<IEnumerable<Article>> ListAsync(...)
{
  var query = BuildQuery(tag, author, favorited);
  return await query
    .Skip(offset)
    .Take(limit)
    .AsNoTracking()
    .ToListAsync();
}

// Handler returns entities
public async Task<Result<ArticlesEntitiesResult>> Handle(...)
{
  var articles = await _query.ListAsync(...);
  return Result.Success(new ArticlesEntitiesResult(articles.ToList(), articles.Count()));
}

// Endpoint maps using FastEndpoints mapper
public override async Task HandleAsync(...)
{
  var result = await _mediator.Send(new ListArticlesQuery(...));
  var articleDtos = result.Value.Articles.Select(article => Map.FromEntity(article).Article).ToList();
  Response = new ArticlesResponse(articleDtos, result.Value.ArticlesCount);
}
```

**Benefits:**
- Consistent mapping approach across single-entity and collection operations
- Infrastructure remains independent (no Application mapper dependencies)
- FastEndpoints mapper centralizes all DTO mapping logic with user context

### Migration Strategy

**Do:**
- Use FastEndpoints ResponseMapper for all entity-to-DTO mappings
- Have handlers return entities (single or collections)
- Map entities at the endpoint layer using FastEndpoints mappers
- Keep Infrastructure independent of Application layer

**Don't:**
- Create static mappers in UseCases for entity-to-DTO mapping
- Call mappers from Infrastructure layer
- Mix different mapping approaches without clear justification

### Mapping Patterns

#### Entity to DTO Mapping
```csharp
public static ArticleDto MapToDto(Article article, User? currentUser = null)
{
    // Calculate context-dependent values (favorited, following)
    var isFavorited = currentUser != null && article.FavoritedBy.Any(u => u.Id == currentUser.Id);
    var isFollowing = currentUser?.IsFollowing(article.AuthorId) ?? false;

    // Return DTO with all required properties
    return new ArticleDto(/* ... */);
}
```

#### Explicit State Overrides
For cases where computed state needs to be overridden (e.g., favorite/unfavorite operations):
```csharp
public static ArticleDto MapToDto(Article article, User? currentUser, bool isFavorited)
{
    // Use explicit favorited state instead of computing from entity
}
```

### Current User Context
Mappers that require user context (for favorited/following status) accept an optional `User? currentUser` parameter:
- **Null currentUser**: Returns default false values for user-dependent fields
- **Non-null currentUser**: Computes actual favorited/following status

### Usage in Handlers
Handlers should use mappers instead of inline mapping:

**Before:**
```csharp
var articleDto = new ArticleDto(
    article.Slug,
    article.Title,
    // ... 20 lines of mapping code
);
```

**After:**
```csharp
var articleDto = ArticleMappers.MapToDto(article, currentUser);
```

### Benefits
1. **Consistency**: All Article-to-ArticleDto mappings use identical logic
2. **Maintainability**: Changes to mapping logic only need to be made in one place
3. **Testability**: Mappers can be unit tested independently
4. **Readability**: Handlers focus on business logic, not mapping details

### Testing Strategy
- Each mapper class has corresponding unit tests in `Server.UnitTests.UseCases.[Feature]`
- Tests verify correct field mapping, handling of null values, and edge cases
- Integration tests continue to validate end-to-end behavior

### Migration Guide
When adding new DTOs or entities:
1. Check if similar mapping logic already exists
2. If duplication is found across 3+ locations, create a mapper
3. Place mapper in appropriate feature namespace
4. Add unit tests for the mapper
5. Update handlers to use the mapper
6. Remove duplicated inline mapping code