using Server.UseCases.Profiles;

namespace Server.Web.Profiles;

public class ProfileMapper : ResponseMapper<ProfileResponse, ProfileResult>
{
  public override Task<ProfileResponse> FromEntityAsync(ProfileResult result, CancellationToken ct)
  {
    var response = new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = result.User.UserName ?? string.Empty,
        Bio = result.User.Bio,
        Image = result.User.Image,
        Following = result.Following,
      },
    };

    return Task.FromResult(response);
  }
}
