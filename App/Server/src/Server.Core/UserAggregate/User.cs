using Audit.EntityFramework;
using Microsoft.AspNetCore.Identity;

namespace Server.Core.UserAggregate;

public class User : IdentityUser<Guid>, IAggregateRoot, IAuditableEntity
{
  // Required parameterless constructor for EF Core
  // Do NOT initialize navigation properties here - EF Core will populate them when loading from database
  private User() : base()
  {
    // Only set defaults for value types and simple properties
    // Navigation properties (Following, Followers) will be initialized by EF Core
  }

  public User(string email, string username, string hashedPassword) : base()
  {
    // Initialize collections and defaults
    Bio = "I work at statefarm";
    Image = null;
    Following = new List<UserFollowing>();
    Followers = new List<UserFollowing>();

    // Set Identity fields
    Id = Guid.NewGuid();
    Email = Guard.Against.NullOrEmpty(email, nameof(email));
    NormalizedEmail = email.ToUpperInvariant();
    UserName = Guard.Against.NullOrEmpty(username, nameof(username));
    NormalizedUserName = username.ToUpperInvariant();
    PasswordHash = Guard.Against.NullOrEmpty(hashedPassword, nameof(hashedPassword));
    SecurityStamp = Guid.NewGuid().ToString();
    EmailConfirmed = false;
    PhoneNumberConfirmed = false;
    TwoFactorEnabled = false;
    LockoutEnabled = false;
    AccessFailedCount = 0;
  }

  // Constructor for creating user without password (used during registration)
  public User(string email, string username) : base()
  {
    // Initialize collections and defaults
    Bio = "I work at statefarm";
    Image = null;
    Following = new List<UserFollowing>();
    Followers = new List<UserFollowing>();

    // Set Identity fields
    Id = Guid.NewGuid();
    Email = Guard.Against.NullOrEmpty(email, nameof(email));
    NormalizedEmail = email.ToUpperInvariant();
    UserName = Guard.Against.NullOrEmpty(username, nameof(username));
    NormalizedUserName = username.ToUpperInvariant();
    SecurityStamp = Guid.NewGuid().ToString();
    EmailConfirmed = false;
    PhoneNumberConfirmed = false;
    TwoFactorEnabled = false;
    LockoutEnabled = false;
    AccessFailedCount = 0;
  }

  // Map UserName to Username for compatibility
  public string Username => UserName ?? string.Empty;

  // Map PasswordHash to HashedPassword for compatibility
  [AuditIgnore]
  public string HashedPassword
  {
    get => PasswordHash ?? string.Empty;
    private set => PasswordHash = value;
  }

  public string Bio { get; private set; } = default!;
  public string? Image { get; private set; }

  // Add audit fields from EntityBase
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string CreatedBy { get; set; } = default!;
  public string UpdatedBy { get; set; } = default!;
  public byte[] ChangeCheck { get; set; } = default!;

  // Navigation properties for following relationships
  // These are initialized in public constructors for new entities
  // EF Core will populate them for loaded entities
  public ICollection<UserFollowing> Following { get; set; } = default!;
  public ICollection<UserFollowing> Followers { get; set; } = default!;

  public User UpdateEmail(string newEmail)
  {
    Email = Guard.Against.NullOrEmpty(newEmail, nameof(newEmail));
    NormalizedEmail = newEmail.ToUpperInvariant();
    return this;
  }

  public User UpdateUsername(string newUsername)
  {
    var username = Guard.Against.NullOrEmpty(newUsername, nameof(newUsername));
    UserName = username;
    NormalizedUserName = username.ToUpperInvariant();
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
    PasswordHash = Guard.Against.NullOrEmpty(newHashedPassword, nameof(newHashedPassword));
    return this;
  }

  public bool IsFollowing(User user)
  {
    return Following.Any(f => f.FollowedId == user.Id);
  }

  public bool IsFollowing(Guid userId)
  {
    return Following.Any(f => f.FollowedId == userId);
  }

  public void Follow(User userToFollow)
  {
    if (Id == userToFollow.Id)
    {
      throw new InvalidOperationException("Cannot follow yourself");
    }

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
