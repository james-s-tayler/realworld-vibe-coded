using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Result;
using Server.UseCases.Users.UpdateRoles;

namespace Server.UnitTests.UseCases.Users;

public class UpdateRolesHandlerTests
{
  private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
    Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

  private readonly IStringLocalizer _localizer = Substitute.For<IStringLocalizer>();
  private readonly UpdateRolesHandler _handler;

  public UpdateRolesHandlerTests()
  {
    var serviceProvider = Substitute.For<IServiceProvider>();
    serviceProvider.GetService(typeof(UserManager<ApplicationUser>)).Returns(_userManager);

    var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    httpContextAccessor.HttpContext.Returns(httpContext);

    _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));

    _handler = new UpdateRolesHandler(
      httpContextAccessor,
      _localizer,
      NullLogger<UpdateRolesHandler>.Instance);
  }

  [Fact]
  public async Task Handle_WhenUserNotFound_ReturnsNotFound()
  {
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

    var command = new UpdateRolesCommand(Guid.NewGuid(), Guid.NewGuid(), [DefaultRoles.Admin]);
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenRemovingOwnAdminRole_ReturnsForbidden()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId };
    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.GetRolesAsync(user).Returns(new List<string> { DefaultRoles.Admin, DefaultRoles.User });

    var command = new UpdateRolesCommand(userId, userId, [DefaultRoles.User]);
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Forbidden);
  }

  [Fact]
  public async Task Handle_WhenValid_ReturnsNoContent()
  {
    var userId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId };
    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.GetRolesAsync(user).Returns(new List<string> { DefaultRoles.User });
    _userManager.AddToRolesAsync(user, Arg.Any<IEnumerable<string>>()).Returns(IdentityResult.Success);

    var command = new UpdateRolesCommand(userId, currentUserId, [DefaultRoles.User, DefaultRoles.Admin]);
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NoContent);
    await _userManager.Received(1).AddToRolesAsync(user, Arg.Is<IEnumerable<string>>(r => r.Contains(DefaultRoles.Admin)));
  }

  [Fact]
  public async Task Handle_WhenNoChanges_ReturnsNoContent()
  {
    var userId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId };
    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.GetRolesAsync(user).Returns(new List<string> { DefaultRoles.Admin, DefaultRoles.User });

    var command = new UpdateRolesCommand(userId, currentUserId, [DefaultRoles.Admin, DefaultRoles.User]);
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NoContent);
    await _userManager.DidNotReceive().AddToRolesAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>());
    await _userManager.DidNotReceive().RemoveFromRolesAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>());
  }
}
