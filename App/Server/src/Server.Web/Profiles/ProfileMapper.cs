using Microsoft.EntityFrameworkCore;
using Server.Core.AuthorAggregate;
using Server.Infrastructure.Data;
using Server.UseCases.Interfaces;

namespace Server.Web.Profiles;

/// <summary>
/// FastEndpoints mapper for Author entity to ProfileResponse DTO
/// Maps domain entity to profile response with current user context for following status
/// </summary>
public class ProfileMapper : ResponseMapper<ProfileResponse, Author>
{
  public override async Task<ProfileResponse> FromEntityAsync(Author author, CancellationToken ct)
  {
    // Resolve current user service to get authentication context
    var currentUserService = Resolve<IUserContext>();
    var currentUserId = currentUserService.GetCurrentUserId();

    // Determine if the current user is following this author
    // Query AuthorFollowing directly to avoid loading ASP.NET Identity fields
    bool isFollowing = false;
    if (currentUserId.HasValue)
    {
      var dbContext = Resolve<AppDbContext>();
      isFollowing = await dbContext.Set<AuthorFollowing>()
        .AnyAsync(af => af.FollowerId == currentUserId.Value && af.FollowedId == author.Id, ct);
    }

    return new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = author.Username,
        Bio = author.Bio,
        Image = author.Image,
        Following = isFollowing,
      },
    };
  }
}
