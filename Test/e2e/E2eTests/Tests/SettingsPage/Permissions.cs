namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Permission tests for the Settings page (/settings).
/// </summary>
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  // No permission tests for SettingsPage currently - protected route tests are in LoginPage
}
