using Server.Core.ContributorAggregate;

namespace Server.IntegrationTests.Data;

public class EfRepositoryDelete : BaseEfRepoTestFixture
{
  [Fact]
  public async Task DeletesItemAfterAddingIt()
  {
    // add a Contributor
    var repository = GetRepository();
    var initialName = Guid.NewGuid().ToString();
    var Contributor = new Contributor(initialName);
    await repository.AddAsync(Contributor, Xunit.TestContext.Current.CancellationToken);

    // delete the item
    await repository.DeleteAsync(Contributor, Xunit.TestContext.Current.CancellationToken);

    // verify it's no longer there
    (await repository.ListAsync(Xunit.TestContext.Current.CancellationToken)).ShouldNotContain(Contributor => Contributor.Name == initialName);
  }
}
