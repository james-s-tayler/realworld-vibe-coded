using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Server.Infrastructure.Services;

namespace Server.UnitTests.Infrastructure;

/// <summary>
/// Tests for UserContext to ensure proper user context management.
/// </summary>
public class UserContextTests
{
  [Fact]
  public void GetCurrentUserId_WhenUserAuthenticated_ShouldReturnUserId()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    var userId = Guid.NewGuid();
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    };
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var result = userContext.GetCurrentUserId();

    // Assert
    result.ShouldBe(userId);
  }

  [Fact]
  public void GetCorrelationId_ShouldReturnGuidStringWhenNoHttpContext()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    httpContextAccessor.HttpContext.Returns((HttpContext?)null);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    correlationId.ShouldBe(Guid.Empty.ToString());
  }

  [Fact]
  public void GetCurrentUsername_WhenUserAuthenticated_ShouldReturnUsername()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.Name, "testuser"),
      new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
    };
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var username = userContext.GetCurrentUsername();

    // Assert
    username.ShouldBe("testuser");
  }

  [Fact]
  public void GetCurrentUsername_WhenNoHttpContext_ShouldReturnNull()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    httpContextAccessor.HttpContext.Returns((HttpContext?)null);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var username = userContext.GetCurrentUsername();

    // Assert
    username.ShouldBeNull();
  }

  [Fact]
  public void GetCurrentUsername_WhenUserNotAuthenticated_ShouldReturnNull()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No authentication type
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var username = userContext.GetCurrentUsername();

    // Assert
    username.ShouldBeNull();
  }
}
