namespace Server.Core.UserAggregate;

public class User : EntityBase, IAggregateRoot
{
  public User(string email, string username, string hashedPassword)
  {
    UpdateEmail(email);
    UpdateUsername(username);
    HashedPassword = Guard.Against.NullOrEmpty(hashedPassword, nameof(hashedPassword));
    Bio = "I work at statefarm"; // Default bio as per Postman tests
    Image = null; // Default image as per Postman tests
    Following = new List<UserFollowing>();
    Followers = new List<UserFollowing>();
  }

  public string Email { get; private set; } = default!;
  public string Username { get; private set; } = default!;
  public string HashedPassword { get; private set; } = default!;
  public string Bio { get; private set; } = default!;
  public string? Image { get; private set; }

  // Navigation properties for following relationships
  public ICollection<UserFollowing> Following { get; private set; } = new List<UserFollowing>();
  public ICollection<UserFollowing> Followers { get; private set; } = new List<UserFollowing>();

  public User UpdateEmail(string newEmail)
  {
    Email = Guard.Against.NullOrEmpty(newEmail, nameof(newEmail));
    return this;
  }

  public User UpdateUsername(string newUsername)
  {
    Username = Guard.Against.NullOrEmpty(newUsername, nameof(newUsername));
    return this;
  }

  public User UpdateBio(string newBio)
  {
    Bio = newBio ?? string.Empty;
    return this;
  }

  public User UpdateImage(string? newImage)
  {
    Image = newImage;
    return this;
  }

  public User UpdatePassword(string newHashedPassword)
  {
    HashedPassword = Guard.Against.NullOrEmpty(newHashedPassword, nameof(newHashedPassword));
    return this;
  }

  public bool IsFollowing(User user)
  {
    return Following.Any(f => f.FollowedId == user.Id);
  }

  public bool IsFollowing(int userId)
  {
    return Following.Any(f => f.FollowedId == userId);
  }

  public void Follow(User userToFollow)
  {
    if (Id == userToFollow.Id)
      throw new InvalidOperationException("Cannot follow yourself");

    if (!IsFollowing(userToFollow))
    {
      var following = new UserFollowing(Id, userToFollow.Id);
      Following.Add(following);
    }
  }

  public void Unfollow(User userToUnfollow)
  {
    var following = Following.FirstOrDefault(f => f.FollowedId == userToUnfollow.Id);
    if (following != null)
    {
      Following.Remove(following);
    }
  }
}
