using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Swagger API documentation page (/swagger/index.html).
/// </summary>
public class SwaggerPage
{
  private readonly IPage _page;
  private readonly string _baseUrl;

  public SwaggerPage(IPage page, string baseUrl)
  {
    _page = page;
    _baseUrl = baseUrl;
  }

  /// <summary>
  /// API info/title section.
  /// </summary>
  public ILocator ApiInfo => _page.Locator(".info .title");

  /// <summary>
  /// Navigates directly to the Swagger documentation page.
  /// </summary>
  public async Task GoToAsync()
  {
    await _page.GotoAsync(_baseUrl);
  }
}
