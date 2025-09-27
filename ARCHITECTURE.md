# Architecture Documentation

## Mapping Conventions

### Overview
To reduce code duplication and ensure consistency across handlers, mapping logic between domain entities and DTOs has been extracted into static mapper classes. These mappers follow a consistent pattern and are organized by feature area.

### Mapper Organization

#### Location Strategy
Mappers are placed within the UseCases project, organized by feature:
- `Server.UseCases.Articles.ArticleMappers` - Article and AuthorDto mapping
- `Server.UseCases.Articles.Comments.CommentMappers` - Comment mapping  
- `Server.UseCases.Users.UserMappers` - User mapping
- `Server.UseCases.Contributors.ContributorMappers` - Contributor mapping

#### Naming Conventions
- Mapper classes: `[Feature]Mappers` (e.g., `ArticleMappers`)
- Main mapping method: `MapToDto(entity, ...)`
- Utility methods: Descriptive names (e.g., `GenerateSlug`)

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