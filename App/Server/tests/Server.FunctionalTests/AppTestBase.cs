namespace Server.FunctionalTests;

public abstract class AppTestBase<TAppFixture> : TestBase<TAppFixture> where TAppFixture : ApiFixtureBase
{
  protected TAppFixture Fixture { get; private set; }

  protected AppTestBase(TAppFixture fixture)
  {
    fixture.SetTestOutputHelper(Output);
    Fixture = fixture;
  }
}
