using Microsoft.AspNetCore.Identity;
using Server.Core.UserAggregate;

namespace Server.Core.IdentityAggregate;

public class ApplicationUser : IdentityUser<Guid>
{
  public const int BioMaxLength = 1000;
  public const int ImageUrlMaxLength = 500;

  public ApplicationUser()
  {
    Bio = "I work at statefarm"; // Default bio as per existing User entity
    Image = null; // Default image as per existing User entity
    Following = new List<UserFollowing>();
    Followers = new List<UserFollowing>();
  }

  public string Bio { get; set; } = default!;

  public string? Image { get; set; }

  // Navigation properties for following relationships
  public ICollection<UserFollowing> Following { get; set; } = new List<UserFollowing>();

  public ICollection<UserFollowing> Followers { get; set; } = new List<UserFollowing>();
}
