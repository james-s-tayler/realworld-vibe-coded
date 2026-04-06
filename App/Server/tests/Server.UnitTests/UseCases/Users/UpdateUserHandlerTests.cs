using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Result;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Dtos;
using Server.UseCases.Users.Update;

namespace Server.UnitTests.UseCases.Users;

public class UpdateUserHandlerTests
{
  private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
    Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

  private readonly IQueryApplicationUsers _queryApplicationUsers = Substitute.For<IQueryApplicationUsers>();
  private readonly IStringLocalizer _localizer = Substitute.For<IStringLocalizer>();
  private readonly UpdateUserHandler _handler;

  public UpdateUserHandlerTests()
  {
    _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));

    _handler = new UpdateUserHandler(
      _userManager,
      _queryApplicationUsers,
      _localizer,
      NullLogger<UpdateUserHandler>.Instance);
  }

  [Fact]
  public async Task Handle_WhenUserNotFound_ReturnsNotFound()
  {
    _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

    var command = new UpdateUserCommand(Guid.NewGuid(), Email: "new@test.com");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenDuplicateEmail_ReturnsInvalid()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId, Email = "old@test.com", UserName = "testuser" };
    var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "taken@test.com" };

    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.FindByEmailAsync("taken@test.com").Returns(existingUser);

    var command = new UpdateUserCommand(userId, Email: "taken@test.com");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Invalid);
  }

  [Fact]
  public async Task Handle_WhenDuplicateUsername_ReturnsInvalid()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId, Email = "test@test.com", UserName = "oldname" };
    var existingUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "takenname" };

    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.FindByNameAsync("takenname").Returns(existingUser);

    var command = new UpdateUserCommand(userId, Username: "takenname");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Invalid);
  }

  [Fact]
  public async Task Handle_WhenValid_ReturnsSuccess()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId, Email = "test@test.com", UserName = "testuser" };

    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

    var dto = new UserWithRolesDto(userId, "test@test.com", "testuser", "bio", null, [DefaultRoles.User], true, "en");
    _queryApplicationUsers.GetCurrentUserWithRoles(userId, Arg.Any<CancellationToken>()).Returns(dto);

    var command = new UpdateUserCommand(userId, Bio: "new bio");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe(dto);
  }

  [Fact]
  public async Task Handle_WhenPasswordUpdateFails_ReturnsInvalid()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId, Email = "test@test.com", UserName = "testuser" };

    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.GeneratePasswordResetTokenAsync(user).Returns("token");
    _userManager.ResetPasswordAsync(user, "token", "weak").Returns(
      IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

    var command = new UpdateUserCommand(userId, Password: "weak");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Invalid);
  }

  [Fact]
  public async Task Handle_WhenUpdateAsyncFails_ReturnsInvalid()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId, Email = "test@test.com", UserName = "testuser" };

    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.UpdateAsync(user).Returns(
      IdentityResult.Failed(new IdentityError { Description = "Concurrency failure" }));

    var command = new UpdateUserCommand(userId, Bio: "new bio");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Invalid);
  }

  [Fact]
  public async Task Handle_WhenUpdatingMultipleFields_ReturnsSuccess()
  {
    var userId = Guid.NewGuid();
    var user = new ApplicationUser { Id = userId, Email = "test@test.com", UserName = "testuser" };

    _userManager.FindByIdAsync(userId.ToString()).Returns(user);
    _userManager.FindByEmailAsync("new@test.com").Returns((ApplicationUser?)null);
    _userManager.FindByNameAsync("newname").Returns((ApplicationUser?)null);
    _userManager.GeneratePasswordResetTokenAsync(user).Returns("token");
    _userManager.ResetPasswordAsync(user, "token", "NewPassword1!").Returns(IdentityResult.Success);
    _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

    var dto = new UserWithRolesDto(userId, "new@test.com", "newname", "bio", "img.png", [DefaultRoles.User], true, "ja");
    _queryApplicationUsers.GetCurrentUserWithRoles(userId, Arg.Any<CancellationToken>()).Returns(dto);

    var command = new UpdateUserCommand(userId, Email: "new@test.com", Username: "newname", Password: "NewPassword1!", Bio: "bio", Image: "img.png", Language: "ja");
    var result = await _handler.Handle(command, CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
  }
}
