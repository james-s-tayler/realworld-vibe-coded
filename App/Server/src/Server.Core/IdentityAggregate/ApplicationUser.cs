using Microsoft.AspNetCore.Identity;
using Server.Core.UserAggregate;

namespace Server.Core.IdentityAggregate;

public class ApplicationUser : IdentityUser<Guid>
{
  public const int EmailMaxLength = 255;
  public const int UsernameMinLength = 2;
  public const int UsernameMaxLength = 100;
  public const int PasswordMinLength = 6;
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

  /// <summary>
  /// Follow another user
  /// </summary>
  public void Follow(ApplicationUser userToFollow)
  {
    if (userToFollow.Id == Id)
    {
      return; // Cannot follow yourself
    }

    if (Following.Any(f => f.FollowedId == userToFollow.Id))
    {
      return; // Already following
    }

    var following = new UserFollowing(Id, userToFollow.Id);
    Following.Add(following);
  }

  /// <summary>
  /// Unfollow a user
  /// </summary>
  public void Unfollow(ApplicationUser userToUnfollow)
  {
    var following = Following.FirstOrDefault(f => f.FollowedId == userToUnfollow.Id);
    if (following != null)
    {
      Following.Remove(following);
    }
  }

  /// <summary>
  /// Check if this user is following another user
  /// </summary>
  public bool IsFollowing(ApplicationUser user)
  {
    return Following.Any(f => f.FollowedId == user.Id);
  }

  /// <summary>
  /// Check if this user is following a user by ID
  /// </summary>
  public bool IsFollowing(Guid userId)
  {
    return Following.Any(f => f.FollowedId == userId);
  }
}
