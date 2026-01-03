using Serilog.Core;
using Serilog.Events;
using Server.Core.IdentityAggregate;
using Server.Web.Infrastructure;

namespace Server.UnitTests.Infrastructure;

public class ApplicationUserDestructuringPolicyTests
{
  private readonly ApplicationUserDestructuringPolicy _policy = new();

  [Fact]
  public void TryDestructure_WithApplicationUser_ShouldReturnTrue()
  {
    var user = new ApplicationUser
    {
      Id = Guid.NewGuid(),
      UserName = "testuser",
      Email = "test@example.com",
      Bio = "Test bio",
      Image = "https://example.com/image.jpg",
      PasswordHash = "SENSITIVE_PASSWORD_HASH",
      SecurityStamp = "SENSITIVE_SECURITY_STAMP",
      ConcurrencyStamp = "SENSITIVE_CONCURRENCY_STAMP",
    };

    var factory = new SimpleLogEventPropertyValueFactory();
    var result = _policy.TryDestructure(user, factory, out var propertyValue);

    result.ShouldBeTrue();
    propertyValue.ShouldNotBeNull();
  }

  [Fact]
  public void TryDestructure_WithApplicationUser_ShouldMaskPasswordHash()
  {
    var user = new ApplicationUser
    {
      Id = Guid.NewGuid(),
      UserName = "testuser",
      Email = "test@example.com",
      PasswordHash = "SENSITIVE_PASSWORD_HASH",
    };

    var factory = new SimpleLogEventPropertyValueFactory();
    _policy.TryDestructure(user, factory, out var propertyValue);

    var structuredValue = (StructureValue)propertyValue;
    var passwordHashProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "PasswordHash");

    passwordHashProperty.ShouldNotBeNull();
    var scalarValue = passwordHashProperty.Value as ScalarValue;
    scalarValue.ShouldNotBeNull();
    scalarValue.Value.ShouldBe("***REDACTED***");
  }

  [Fact]
  public void TryDestructure_WithApplicationUser_ShouldMaskSecurityStamp()
  {
    var user = new ApplicationUser
    {
      Id = Guid.NewGuid(),
      UserName = "testuser",
      Email = "test@example.com",
      SecurityStamp = "SENSITIVE_SECURITY_STAMP",
    };

    var factory = new SimpleLogEventPropertyValueFactory();
    _policy.TryDestructure(user, factory, out var propertyValue);

    var structuredValue = (StructureValue)propertyValue;
    var securityStampProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "SecurityStamp");

    securityStampProperty.ShouldNotBeNull();
    var scalarValue = securityStampProperty.Value as ScalarValue;
    scalarValue.ShouldNotBeNull();
    scalarValue.Value.ShouldBe("***REDACTED***");
  }

  [Fact]
  public void TryDestructure_WithApplicationUser_ShouldMaskConcurrencyStamp()
  {
    var user = new ApplicationUser
    {
      Id = Guid.NewGuid(),
      UserName = "testuser",
      Email = "test@example.com",
      ConcurrencyStamp = "SENSITIVE_CONCURRENCY_STAMP",
    };

    var factory = new SimpleLogEventPropertyValueFactory();
    _policy.TryDestructure(user, factory, out var propertyValue);

    var structuredValue = (StructureValue)propertyValue;
    var concurrencyStampProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "ConcurrencyStamp");

    concurrencyStampProperty.ShouldNotBeNull();
    var scalarValue = concurrencyStampProperty.Value as ScalarValue;
    scalarValue.ShouldNotBeNull();
    scalarValue.Value.ShouldBe("***REDACTED***");
  }

  [Fact]
  public void TryDestructure_WithApplicationUser_ShouldIncludeSafeProperties()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser
    {
      Id = userId,
      UserName = "testuser",
      Email = "test@example.com",
      Bio = "Test bio",
      Image = "https://example.com/image.jpg",
      PasswordHash = "SENSITIVE_PASSWORD_HASH",
    };

    var factory = new SimpleLogEventPropertyValueFactory();
    _policy.TryDestructure(user, factory, out var propertyValue);

    var structuredValue = (StructureValue)propertyValue;

    var idProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "Id");
    idProperty.ShouldNotBeNull();
    ((ScalarValue)idProperty.Value).Value.ShouldBe(userId);

    var usernameProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "UserName");
    usernameProperty.ShouldNotBeNull();
    ((ScalarValue)usernameProperty.Value).Value.ShouldBe("testuser");

    var emailProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "Email");
    emailProperty.ShouldNotBeNull();
    ((ScalarValue)emailProperty.Value).Value.ShouldBe("test@example.com");

    var bioProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "Bio");
    bioProperty.ShouldNotBeNull();
    ((ScalarValue)bioProperty.Value).Value.ShouldBe("Test bio");

    var imageProperty = structuredValue.Properties.FirstOrDefault(p => p.Name == "Image");
    imageProperty.ShouldNotBeNull();
    ((ScalarValue)imageProperty.Value).Value.ShouldBe("https://example.com/image.jpg");
  }

  [Fact]
  public void TryDestructure_WithNonApplicationUser_ShouldReturnFalse()
  {
    var nonUser = new { Name = "Not a user" };

    var factory = new SimpleLogEventPropertyValueFactory();
    var result = _policy.TryDestructure(nonUser, factory, out var propertyValue);

    result.ShouldBeFalse();
    propertyValue.ShouldBeNull();
  }

  private class SimpleLogEventPropertyValueFactory : ILogEventPropertyValueFactory
  {
    public LogEventPropertyValue CreatePropertyValue(object? value, bool destructureObjects = false)
    {
      if (value is null)
      {
        return new ScalarValue(null);
      }

      if (value is IDictionary<string, object?> dict)
      {
        var properties = dict.Select(kvp =>
          new LogEventProperty(kvp.Key, CreatePropertyValue(kvp.Value, destructureObjects)));
        return new StructureValue(properties);
      }

      return new ScalarValue(value);
    }
  }
}
