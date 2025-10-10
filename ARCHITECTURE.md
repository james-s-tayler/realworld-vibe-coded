# Architecture Documentation

## Mapping Conventions

### Overview
To reduce code duplication and ensure consistency across handlers, mapping logic between domain entities and DTOs has been extracted into mapper classes following the [FastEndpoints Domain Entity Mapping](https://fast-endpoints.com/docs/domain-entity-mapping) pattern.

Our mappers are instance-based classes (not static) that accept context (e.g., current user) via constructor injection, aligning with FastEndpoints' recommended approach where mapping logic resides in the Application layer (UseCases), not in the Infrastructure layer.

### Mapper Organization

#### Location Strategy
Mappers are placed within the UseCases project, organized by feature:
- `Server.UseCases.Articles.ArticleResponseMapper` - Article to Response mapping (FastEndpoints-style)
- `Server.UseCases.Articles.ArticleMappers` - Article utility methods (GenerateSlug)
- `Server.UseCases.Articles.Comments.CommentMappers` - Comment mapping  
- `Server.UseCases.Users.UserMappers` - User mapping
- `Server.UseCases.Contributors.ContributorMappers` - Contributor mapping

#### Naming Conventions
- Mapper classes: `[Feature]ResponseMapper` for FastEndpoints-style mappers (e.g., `ArticleResponseMapper`)
- Legacy static mappers: `[Feature]Mappers` for utility functions only
- Mapping methods: `FromEntity(entity)` - Follows FastEndpoints convention
- Utility methods: Descriptive names (e.g., `GenerateSlug`)

### Mapping Patterns

#### FastEndpoints-Style Mapper Pattern
Following FastEndpoints Domain Entity Mapping, our mappers are instance-based classes that accept context via constructor:

```csharp
public class ArticleResponseMapper
{
  private readonly User? _currentUser;

  public ArticleResponseMapper(User? currentUser = null)
  {
    _currentUser = currentUser;
  }

  public ArticleResponse FromEntity(Article article)
  {
    // Calculate context-dependent values (favorited, following)
    var isFavorited = _currentUser != null && article.FavoritedBy.Any(u => u.Id == _currentUser.Id);
    var isFollowing = _currentUser?.IsFollowing(article.AuthorId) ?? false;

    // Return DTO with all required properties
    return new ArticleResponse { Article = new ArticleDto(/* ... */) };
  }
}
```

#### Explicit State Overrides
For cases where computed state needs to be overridden (e.g., favorite/unfavorite operations):
```csharp
public ArticleResponse FromEntity(Article article, bool isFavorited)
{
    // Use explicit favorited state instead of computing from entity
}
```

### Current User Context
Following the FastEndpoints pattern, mappers accept user context via constructor injection:
- **Null currentUser**: Returns default false values for user-dependent fields
- **Non-null currentUser**: Computes actual favorited/following status

### Usage in Handlers
Handlers instantiate mappers with context and use the `FromEntity` method:

**Before (inline mapping):**
```csharp
var articleDto = new ArticleDto(
    article.Slug,
    article.Title,
    // ... 20 lines of mapping code
);
```

**After (FastEndpoints-style mapper):**
```csharp
var mapper = new ArticleResponseMapper(currentUser);
var response = mapper.FromEntity(article);
return Result.Success(response);
```

### Benefits
1. **FastEndpoints Alignment**: Follows the recommended Domain Entity Mapping pattern from FastEndpoints documentation
2. **Consistency**: All Article-to-Response mappings use identical logic
3. **Maintainability**: Changes to mapping logic only need to be made in one place
4. **Testability**: Instance-based mappers can be easily unit tested with different contexts
5. **Readability**: Handlers focus on business logic, not mapping details
6. **Context Injection**: User context is injected once via constructor, not passed to every method

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

### Architecture Guidelines

#### Separation of Concerns
Following clean architecture principles and the FastEndpoints pattern:

1. **Infrastructure Layer**: 
   - Returns domain entities OR performs direct LINQ/SQL projections to read models
   - Does NOT call Application layer mappers
   - Does NOT depend on Application layer for mapping logic

2. **Application Layer (UseCases/Handlers)**:
   - Performs all DTO mapping using mapper classes (for complex mappings)
   - Maps domain entities to response DTOs
   - Maps request DTOs to domain entities (for commands)

3. **Benefits**:
   - Clear dependency direction (Infrastructure → Core, Application → Core)
   - Infrastructure remains focused on data access
   - Mapping logic centralized in Application layer
   - Easier to test and maintain

#### Supported Patterns

We support two patterns for read operations:

##### Pattern 1: Domain Entities with Application Mapping (Recommended for Complex Scenarios)

**Infrastructure Query Service:**
```csharp
public async Task<IEnumerable<Article>> ListAsync(...)
{
    var query = BuildQuery(...);
    var articles = await query
        .AsNoTracking()
        .ToListAsync();
    
    return articles; // Return entities, not DTOs
}
```

**Application Handler:**
```csharp
public async Task<Result<ArticlesResponse>> Handle(...)
{
    // Get entities from Infrastructure
    var articles = await _query.ListAsync(...);
    
    // Get user context if needed
    var currentUser = await _userRepository.FirstOrDefaultAsync(...);
    
    // Map entities to DTOs in Application layer
    var articleDtos = articles.Select(a => ArticleMappers.MapToDto(a, currentUser)).ToList();
    
    return Result.Success(new ArticlesResponse(articleDtos, articleDtos.Count));
}
```

**Use when:**
- DTOs require computed values based on user context (favorited, following)
- Complex domain logic is needed for mapping
- Full aggregate loading is necessary for business rules

##### Pattern 2: Direct Projection to Read Models (Recommended for Simple Queries)

**Infrastructure Query Service:**
```csharp
public async Task<IEnumerable<ContributorDTO>> ListAsync()
{
    // Project directly to DTO shape using LINQ or SQL
    var result = await _db.Database.SqlQuery<ContributorDTO>(
        $"SELECT Id, Name, PhoneNumber_Number AS PhoneNumber FROM Contributors")
        .ToListAsync();
    
    return result;
}
```

**Use when:**
- Read-only operations without user context
- Simple field mapping without complex logic
- Performance optimization (avoid loading full aggregates)
- DTOs don't require computed values