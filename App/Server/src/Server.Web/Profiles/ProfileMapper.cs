using Server.Core.IdentityAggregate;

namespace Server.Web.Profiles;

public class ProfileMapper : ResponseMapper<ProfileResponse, ApplicationUser>
{
  public override Task<ProfileResponse> FromEntityAsync(ApplicationUser user, CancellationToken ct)
  {
    var response = new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = user.UserName ?? string.Empty,
        Bio = user.Bio,
        Image = user.Image,
      },
    };

    return Task.FromResult(response);
  }
}
