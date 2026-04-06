using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Dashboard page (/).
/// </summary>
public class DashboardPage : BasePage
{
  public DashboardPage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Welcome heading on the dashboard.
  /// </summary>
  public ILocator WelcomeHeading => Page.GetByRole(AriaRole.Heading, new() { Level = 1 });

  /// <summary>
  /// Feature flag banner notification.
  /// </summary>
  public ILocator FeatureBanner => Page.GetByText("New Feature");
}
