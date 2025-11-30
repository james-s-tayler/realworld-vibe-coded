using Server.Infrastructure.Data;

namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Endpoint that clears all test data from the database.
/// Used by E2E tests to ensure a clean state between tests.
/// </summary>
/// <remarks>
/// Deletes data in the correct order to respect foreign key relationships:
/// 1. UserFollowings (references Users)
/// 2. Comments (references Articles and Users)
/// 3. Clear Article relationships (Tags, FavoritedBy)
/// 4. Articles (references Users)
/// 5. Tags (no dependencies after articles cleared)
/// 6. Users (no dependencies after above deletions)
/// </remarks>
public class ResetDatabase : Endpoint<EmptyRequest>
{
  private readonly AppDbContext _dbContext;

  public ResetDatabase(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public override void Configure()
  {
    Delete("reset");
    Group<TestData>();
    Summary(s =>
    {
      s.Summary = "Reset database - clear all test data";
      s.Description = "Clears all data from the database. Used by E2E tests for cleanup.";
    });
  }

  public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
  {
    // Delete in order that respects foreign key constraints

    // 1. Clear UserFollowings (references Users via FollowerId and FollowedId)
    var userFollowings = await _dbContext.UserFollowings.ToListAsync(cancellationToken);
    _dbContext.UserFollowings.RemoveRange(userFollowings);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // 2. Clear Comments (references Articles and Users)
    var comments = await _dbContext.Comments.ToListAsync(cancellationToken);
    _dbContext.Comments.RemoveRange(comments);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // 3. Load all articles with their relationships and clear them
    var articles = await _dbContext.Articles
      .Include(a => a.Tags)
      .Include(a => a.FavoritedBy)
      .ToListAsync(cancellationToken);

    // Clear article-tag and article-favorite relationships
    foreach (var article in articles)
    {
      article.Tags.Clear();
      article.FavoritedBy.Clear();
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    // 4. Clear Articles (now that relationships are cleared)
    _dbContext.Articles.RemoveRange(articles);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // 5. Clear Tags (no foreign key dependencies after articles cleared)
    var tags = await _dbContext.Tags.ToListAsync(cancellationToken);
    _dbContext.Tags.RemoveRange(tags);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // 6. Clear Users (no foreign key dependencies after above deletions)
    var users = await _dbContext.Users.ToListAsync(cancellationToken);
    _dbContext.Users.RemoveRange(users);
    await _dbContext.SaveChangesAsync(cancellationToken);

    await HttpContext.Response.SendNoContentAsync(cancellation: cancellationToken);
  }
}
