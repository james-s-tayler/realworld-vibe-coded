namespace Server.FunctionalTests;

public abstract class AppTestBase
{
  protected ApiFixture Fixture { get; }

  protected ITestOutputHelper Output { get; }

  protected AppTestBase(ApiFixture apiFixture, ITestOutputHelper output)
  {
    Output = output;
    apiFixture.SetTestOutputHelper(Output);
    Fixture = apiFixture;
  }
}
