using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Result;
using Server.UseCases.Users.Reactivate;

namespace Server.UnitTests.UseCases.Users;

public class ReactivateUserHandlerTests
{
  private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
    Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

  private readonly ReactivateUserHandler _handler;

  public ReactivateUserHandlerTests()
  {
    var serviceProvider = Substitute.For<IServiceProvider>();
    serviceProvider.GetService(typeof(UserManager<ApplicationUser>)).Returns(_userManager);

    var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    httpContextAccessor.HttpContext.Returns(httpContext);

    _handler = new ReactivateUserHandler(
      httpContextAccessor,
      NullLogger<ReactivateUserHandler>.Instance);
  }

  [Fact]
  public async Task Handle_WhenUserNotFound_ReturnsNotFound()
  {
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

    var command = new ReactivateUserCommand(Guid.NewGuid());
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenValid_ReturnsNoContent()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
    _userManager.SetLockoutEndDateAsync(user, null).Returns(IdentityResult.Success);
    _userManager.ResetAccessFailedCountAsync(user).Returns(IdentityResult.Success);

    var command = new ReactivateUserCommand(user.Id);
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NoContent);
    await _userManager.Received(1).SetLockoutEndDateAsync(user, null);
    await _userManager.Received(1).ResetAccessFailedCountAsync(user);
  }
}
