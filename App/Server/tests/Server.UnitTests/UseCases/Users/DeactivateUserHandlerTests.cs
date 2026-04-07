using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Result;
using Server.UseCases.Users.Deactivate;

namespace Server.UnitTests.UseCases.Users;

public class DeactivateUserHandlerTests
{
  private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
    Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

  private readonly IStringLocalizer _localizer = Substitute.For<IStringLocalizer>();
  private readonly DeactivateUserHandler _handler;

  public DeactivateUserHandlerTests()
  {
    var serviceProvider = Substitute.For<IServiceProvider>();
    serviceProvider.GetService(typeof(UserManager<ApplicationUser>)).Returns(_userManager);

    var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    httpContextAccessor.HttpContext.Returns(httpContext);

    _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));

    _handler = new DeactivateUserHandler(
      httpContextAccessor,
      _localizer,
      NullLogger<DeactivateUserHandler>.Instance);
  }

  [Fact]
  public async Task Handle_WhenSelfDeactivation_ReturnsForbidden()
  {
    var userId = Guid.NewGuid();
    var command = new DeactivateUserCommand(userId, userId);

    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Forbidden);
  }

  [Fact]
  public async Task Handle_WhenUserNotFound_ReturnsNotFound()
  {
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

    var command = new DeactivateUserCommand(Guid.NewGuid(), Guid.NewGuid());
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenTargetIsOwner_ReturnsForbidden()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
    _userManager.GetRolesAsync(user).Returns(new List<string> { DefaultRoles.Owner });

    var command = new DeactivateUserCommand(user.Id, Guid.NewGuid());
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Forbidden);
  }

  [Fact]
  public async Task Handle_WhenValid_ReturnsNoContent()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user);
    _userManager.GetRolesAsync(user).Returns(new List<string> { DefaultRoles.User });
    _userManager.SetLockoutEnabledAsync(user, true).Returns(IdentityResult.Success);
    _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue).Returns(IdentityResult.Success);

    var command = new DeactivateUserCommand(user.Id, Guid.NewGuid());
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NoContent);
    await _userManager.Received(1).SetLockoutEnabledAsync(user, true);
    await _userManager.Received(1).SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
  }
}
