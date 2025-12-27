using Finbuckle.MultiTenant.Abstractions;
using Server.Infrastructure;

namespace Server.UnitTests.Infrastructure;

public class DefaultTenantContextAccessorTests
{
  [Fact]
  public void Constructor_InitializesMultiTenantContext()
  {
    // Act
    var accessor = new DefaultTenantContextAccessor();

    // Assert
    accessor.MultiTenantContext.ShouldNotBeNull();
  }

  [Fact]
  public void MultiTenantContext_HasTenantInfo()
  {
    // Arrange
    var accessor = new DefaultTenantContextAccessor();

    // Act
    var tenantInfo = accessor.MultiTenantContext.TenantInfo;

    // Assert
    tenantInfo.ShouldNotBeNull();
  }

  [Fact]
  public void TenantInfo_HasEmptyStringId()
  {
    // Arrange
    var accessor = new DefaultTenantContextAccessor();

    // Act
    var tenantInfo = accessor.MultiTenantContext.TenantInfo;

    // Assert
    tenantInfo!.Id.ShouldBe(string.Empty);
  }

  [Fact]
  public void TenantInfo_HasEmptyStringIdentifier()
  {
    // Arrange
    var accessor = new DefaultTenantContextAccessor();

    // Act
    var tenantInfo = accessor.MultiTenantContext.TenantInfo;

    // Assert
    tenantInfo!.Identifier.ShouldBe(string.Empty);
  }

  [Fact]
  public void TenantInfo_HasDefaultName()
  {
    // Arrange
    var accessor = new DefaultTenantContextAccessor();

    // Act
    var tenantInfo = accessor.MultiTenantContext.TenantInfo;

    // Assert
    tenantInfo!.Name.ShouldBe("Default");
  }

  [Fact]
  public void DefaultTenantContextAccessor_ImplementsIMultiTenantContextAccessor()
  {
    // Arrange & Act
    var accessor = new DefaultTenantContextAccessor();

    // Assert
    accessor.ShouldBeAssignableTo<IMultiTenantContextAccessor>();
  }

  [Fact]
  public void MultipleInstances_ReturnSeparateContexts()
  {
    // Arrange
    var accessor1 = new DefaultTenantContextAccessor();
    var accessor2 = new DefaultTenantContextAccessor();

    // Act
    var context1 = accessor1.MultiTenantContext;
    var context2 = accessor2.MultiTenantContext;

    // Assert
    context1.ShouldNotBeSameAs(context2);
  }
}
