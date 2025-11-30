using Server.Infrastructure.Data;

namespace Server.Web.DevOnly.UseCases;

/// <summary>
/// Handler that clears all test data from the database.
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
#pragma warning disable SRV015 // DevOnly test endpoint
#pragma warning disable PV002 // DevOnly test endpoint allows DbContext access
#pragma warning disable PV051 // DevOnly test endpoint allows infrastructure injection
#pragma warning disable PV014 // DevOnly test endpoint uses DbContext directly
public class ResetDatabaseHandler(AppDbContext dbContext) : SharedKernel.MediatR.ICommandHandler<ResetDatabaseCommand, Unit>
{
  public async Task<Result<Unit>> Handle(ResetDatabaseCommand request, CancellationToken cancellationToken)
  {
    // Delete in order that respects foreign key constraints

    // 1. Clear UserFollowings (references Users via FollowerId and FollowedId)
    var userFollowings = await dbContext.UserFollowings.ToListAsync(cancellationToken);
    dbContext.UserFollowings.RemoveRange(userFollowings);
    await dbContext.SaveChangesAsync(cancellationToken);

    // 2. Clear Comments (references Articles and Users)
    var comments = await dbContext.Comments.ToListAsync(cancellationToken);
    dbContext.Comments.RemoveRange(comments);
    await dbContext.SaveChangesAsync(cancellationToken);

    // 3. Load all articles with their relationships and clear them
    var articles = await dbContext.Articles
      .Include(a => a.Tags)
      .Include(a => a.FavoritedBy)
      .ToListAsync(cancellationToken);

    // Clear article-tag and article-favorite relationships
    foreach (var article in articles)
    {
      article.Tags.Clear();
      article.FavoritedBy.Clear();
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    // 4. Clear Articles (now that relationships are cleared)
    dbContext.Articles.RemoveRange(articles);
    await dbContext.SaveChangesAsync(cancellationToken);

    // 5. Clear Tags (no foreign key dependencies after articles cleared)
    var tags = await dbContext.Tags.ToListAsync(cancellationToken);
    dbContext.Tags.RemoveRange(tags);
    await dbContext.SaveChangesAsync(cancellationToken);

    // 6. Clear Users (no foreign key dependencies after above deletions)
    var users = await dbContext.Users.ToListAsync(cancellationToken);
    dbContext.Users.RemoveRange(users);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Result<Unit>.NoContent();
  }
}
#pragma warning restore SRV015
#pragma warning restore PV002
#pragma warning restore PV051
#pragma warning restore PV014
