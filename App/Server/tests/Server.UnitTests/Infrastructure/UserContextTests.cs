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
  public void GetCorrelationId_ShouldReturnHeaderValue_WhenHeaderIsPresent()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    var expectedCorrelationId = Guid.NewGuid().ToString();
    httpContext.Request.Headers["x-correlation-id"] = expectedCorrelationId;
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    correlationId.ShouldBe(expectedCorrelationId);
  }

  [Fact]
  public void GetCorrelationId_ShouldStoreHeaderValueInItems_WhenHeaderIsPresent()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    var expectedCorrelationId = Guid.NewGuid().ToString();
    httpContext.Request.Headers["x-correlation-id"] = expectedCorrelationId;
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    userContext.GetCorrelationId();

    // Assert
    httpContext.Items["CorrelationId"].ShouldBe(expectedCorrelationId);
  }

  [Fact]
  public void GetCorrelationId_ShouldReturnItemsValue_WhenHeaderAbsentButItemsPresent()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    var expectedCorrelationId = Guid.NewGuid().ToString();
    httpContext.Items["CorrelationId"] = expectedCorrelationId;
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    correlationId.ShouldBe(expectedCorrelationId);
  }

  [Fact]
  public void GetCorrelationId_ShouldGenerateNewGuid_WhenNeitherHeaderNorItemsPresent()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    Guid.TryParse(correlationId, out var guid).ShouldBeTrue();
    guid.ShouldNotBe(Guid.Empty);
    httpContext.Items["CorrelationId"].ShouldBe(correlationId);
  }

  [Fact]
  public void GetCorrelationId_ShouldReturnSameValue_OnMultipleCalls()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId1 = userContext.GetCorrelationId();
    var correlationId2 = userContext.GetCorrelationId();

    // Assert
    correlationId1.ShouldBe(correlationId2);
  }

  [Fact]
  public void GetCorrelationId_ShouldPreferHeaderOverItems()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    var headerCorrelationId = Guid.NewGuid().ToString();
    var itemsCorrelationId = Guid.NewGuid().ToString();
    httpContext.Request.Headers["x-correlation-id"] = headerCorrelationId;
    httpContext.Items["CorrelationId"] = itemsCorrelationId;
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    correlationId.ShouldBe(headerCorrelationId);
    httpContext.Items["CorrelationId"].ShouldBe(headerCorrelationId);
  }

  [Fact]
  public void GetCorrelationId_ShouldGenerateNewGuid_WhenHeaderIsEmpty()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["x-correlation-id"] = string.Empty;
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    Guid.TryParse(correlationId, out var guid).ShouldBeTrue();
    guid.ShouldNotBe(Guid.Empty);
  }

  [Fact]
  public void GetCorrelationId_ShouldGenerateNewGuid_WhenHeaderIsWhitespace()
  {
    // Arrange
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["x-correlation-id"] = "   ";
    httpContextAccessor.HttpContext.Returns(httpContext);

    var userContext = new UserContext(httpContextAccessor);

    // Act
    var correlationId = userContext.GetCorrelationId();

    // Assert
    Guid.TryParse(correlationId, out var guid).ShouldBeTrue();
    guid.ShouldNotBe(Guid.Empty);
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
