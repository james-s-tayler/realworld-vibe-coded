using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Server.Infrastructure.Services;

namespace Server.UnitTests.Infrastructure.Services;

public class UserContextTests
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly UserContext _userContext;
  private readonly HttpContext _httpContext;

  public UserContextTests()
  {
    _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    _httpContext = Substitute.For<HttpContext>();
    _userContext = new UserContext(_httpContextAccessor);
  }

  [Fact]
  public void GetCurrentUserId_WhenUserNotAuthenticated_ReturnsNull()
  {
    // Arrange
    var user = new ClaimsPrincipal();
    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.GetCurrentUserId();

    // Assert
    result.ShouldBeNull();
  }

  [Fact]
  public void GetCurrentUserId_WhenHttpContextIsNull_ReturnsNull()
  {
    // Arrange
    _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

    // Act
    var result = _userContext.GetCurrentUserId();

    // Assert
    result.ShouldBeNull();
  }

  [Fact]
  public void GetCurrentUserId_WhenUserHasValidUserId_ReturnsUserId()
  {
    // Arrange
    var expectedUserId = Guid.NewGuid();
    var identity = new ClaimsIdentity(
      new[]
      {
        new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
      },
      "test");
    var user = new ClaimsPrincipal(identity);

    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.GetCurrentUserId();

    // Assert
    result.ShouldBe(expectedUserId);
  }

  [Fact]
  public void GetCurrentUserId_WhenUserIdClaimIsInvalid_ReturnsNull()
  {
    // Arrange
    var identity = new ClaimsIdentity(
      new[]
      {
        new Claim(ClaimTypes.NameIdentifier, "invalid"),
      },
      "test");
    var user = new ClaimsPrincipal(identity);

    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.GetCurrentUserId();

    // Assert
    result.ShouldBeNull();
  }

  [Fact]
  public void GetCurrentUserId_WhenUserIdClaimIsMissing_ReturnsNull()
  {
    // Arrange
    var identity = new ClaimsIdentity(
      new[]
      {
        new Claim(ClaimTypes.Name, "testuser"),
      },
      "test");
    var user = new ClaimsPrincipal(identity);

    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.GetCurrentUserId();

    // Assert
    result.ShouldBeNull();
  }

  [Fact]
  public void GetRequiredCurrentUserId_WhenUserAuthenticated_ReturnsUserId()
  {
    // Arrange
    var expectedUserId = Guid.NewGuid();
    var identity = new ClaimsIdentity(
      new[]
      {
        new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
      },
      "test");
    var user = new ClaimsPrincipal(identity);

    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.GetRequiredCurrentUserId();

    // Assert
    result.ShouldBe(expectedUserId);
  }

  [Fact]
  public void GetRequiredCurrentUserId_WhenUserNotAuthenticated_ThrowsException()
  {
    // Arrange
    var user = new ClaimsPrincipal();
    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act & Assert
    var exception = Should.Throw<UnauthorizedAccessException>(() => _userContext.GetRequiredCurrentUserId());
    exception.Message.ShouldBe("User is not authenticated or user ID is invalid");
  }

  [Fact]
  public void IsAuthenticated_WhenUserAuthenticated_ReturnsTrue()
  {
    // Arrange
    var identity = new ClaimsIdentity(
      new[]
      {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
      },
      "test");
    var user = new ClaimsPrincipal(identity);

    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.IsAuthenticated();

    // Assert
    result.ShouldBeTrue();
  }

  [Fact]
  public void IsAuthenticated_WhenUserNotAuthenticated_ReturnsFalse()
  {
    // Arrange
    var user = new ClaimsPrincipal();
    _httpContext.User.Returns(user);
    _httpContextAccessor.HttpContext.Returns(_httpContext);

    // Act
    var result = _userContext.IsAuthenticated();

    // Assert
    result.ShouldBeFalse();
  }
}
