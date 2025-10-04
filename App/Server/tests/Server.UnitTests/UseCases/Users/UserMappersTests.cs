using Server.Core.UserAggregate;
using Server.UseCases.Users;

namespace Server.UnitTests.UseCases.Users;

/// <summary>
/// Unit tests for UserMappers to ensure proper mapping behavior
/// </summary>
public class UserMappersTests
{
  [Fact]
  public void MapToDto_Should_Map_User_To_UserDto_With_Token()
  {
    // Arrange
    var user = new User("test@test.com", "testuser", "hashedpassword");
    const string token = "jwt-token-123";

    // Act
    var result = UserMappers.MapToDto(user, token);

    // Assert
    Assert.Equal(user.Id, result.Id);
    Assert.Equal(user.Email, result.Email);
    Assert.Equal(user.Username, result.Username);
    Assert.Equal(user.Bio, result.Bio);
    Assert.Equal(user.Image, result.Image);
    Assert.Equal(token, result.Token);
  }

  [Fact]
  public void MapToDto_Should_Handle_Null_Bio_And_Image()
  {
    // Arrange
    var user = new User("test@test.com", "testuser", "hashedpassword");
    const string token = "jwt-token-123";

    // Act
    var result = UserMappers.MapToDto(user, token);

    // Assert
    Assert.Equal(user.Bio, result.Bio); // Should be null or empty based on User entity behavior
    Assert.Equal(user.Image, result.Image); // Should be null based on User entity behavior
  }
}
