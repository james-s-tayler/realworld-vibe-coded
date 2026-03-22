using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Result;
using Server.UseCases.Profiles.Get;

namespace Server.UnitTests.UseCases.Profiles;

public class GetProfileHandlerTests
{
  private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
    Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

  [Fact]
  public async Task Handle_WhenUserNotFound_ReturnsNotFound()
  {
    _userManager.FindByNameAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

    var handler = new GetProfileHandler(_userManager);
    var result = await handler.Handle(new GetProfileQuery("unknown"), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenUserFound_ReturnsSuccess()
  {
    var user = new ApplicationUser { UserName = "testuser", Bio = "bio" };
    _userManager.FindByNameAsync("testuser").Returns(user);

    var handler = new GetProfileHandler(_userManager);
    var result = await handler.Handle(new GetProfileQuery("testuser"), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.UserName.ShouldBe("testuser");
  }
}
