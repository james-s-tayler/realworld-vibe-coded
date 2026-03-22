using Server.Core.IdentityAggregate;

namespace Server.UnitTests.Core.IdentityAggregate;

public class ApplicationUserTests
{
  [Fact]
  public void Constructor_SetsDefaultBioAndImage()
  {
    var user = new ApplicationUser();

    user.Bio.ShouldBe("I work at statefarm");
    user.Image.ShouldBeNull();
  }
}
