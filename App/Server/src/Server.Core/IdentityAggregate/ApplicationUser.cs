using Microsoft.AspNetCore.Identity;

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
  }

  public string Bio { get; set; } = default!;

  public string? Image { get; set; }
}
