using Finbuckle.MultiTenant.Abstractions;
using Server.Web.Services;
using TenantInfo = Server.Core.TenantInfoAggregate.TenantInfo;

namespace Server.UnitTests.Services;

public class TenantTargetingContextAccessorTests
{
  [Fact]
  public async Task GetContextAsync_WithTenant_ReturnsTenantIdAsUserId()
  {
    var tenantId = Guid.NewGuid().ToString();
    var contextAccessor = CreateAccessorWithTenant(tenantId);
    var sut = new TenantTargetingContextAccessor(contextAccessor);

    var result = await sut.GetContextAsync();

    result.UserId.ShouldBe(tenantId);
    result.Groups.ShouldBeEmpty();
  }

  [Fact]
  public async Task GetContextAsync_WithNoTenant_ReturnsEmptyUserId()
  {
    var contextAccessor = CreateAccessorWithoutTenant();
    var sut = new TenantTargetingContextAccessor(contextAccessor);

    var result = await sut.GetContextAsync();

    result.UserId.ShouldBe(string.Empty);
    result.Groups.ShouldBeEmpty();
  }

  [Fact]
  public async Task GetContextAsync_WithNullContext_ReturnsEmptyUserId()
  {
    var contextAccessor = Substitute.For<IMultiTenantContextAccessor<TenantInfo>>();
    contextAccessor.MultiTenantContext.Returns((IMultiTenantContext<TenantInfo>?)null);
    var sut = new TenantTargetingContextAccessor(contextAccessor);

    var result = await sut.GetContextAsync();

    result.UserId.ShouldBe(string.Empty);
    result.Groups.ShouldBeEmpty();
  }

  private static IMultiTenantContextAccessor<TenantInfo> CreateAccessorWithTenant(string tenantId)
  {
    var tenantInfo = new TenantInfo { Id = tenantId };
    var multiTenantContext = new MultiTenantContext<TenantInfo>(tenantInfo);

    var accessor = Substitute.For<IMultiTenantContextAccessor<TenantInfo>>();
    accessor.MultiTenantContext.Returns(multiTenantContext);
    return accessor;
  }

  private static IMultiTenantContextAccessor<TenantInfo> CreateAccessorWithoutTenant()
  {
    var multiTenantContext = Substitute.For<IMultiTenantContext<TenantInfo>>();
    multiTenantContext.TenantInfo.Returns((TenantInfo?)null);

    var accessor = Substitute.For<IMultiTenantContextAccessor<TenantInfo>>();
    accessor.MultiTenantContext.Returns(multiTenantContext);
    return accessor;
  }
}
