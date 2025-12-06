using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Swagger API documentation page (/swagger/index.html).
/// </summary>
public class SwaggerPage
{
  private const int DefaultTimeout = 10000;
  private readonly IPage _page;
  private readonly string _baseUrl;

  public SwaggerPage(IPage page, string baseUrl)
  {
    _page = page;
    _baseUrl = baseUrl;
  }

  /// <summary>
  /// Gets an assertion helper for a locator.
  /// </summary>
  protected ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);

  /// <summary>
  /// Swagger UI container.
  /// </summary>
  public ILocator SwaggerContainer => _page.Locator(".swagger-ui").First;

  /// <summary>
  /// API info/title section.
  /// </summary>
  public ILocator ApiInfo => _page.Locator(".info .title");

  /// <summary>
  /// Scheme container section.
  /// </summary>
  public ILocator SchemeContainer => _page.Locator(".scheme-container");

  /// <summary>
  /// Navigates directly to the Swagger documentation page.
  /// </summary>
  public async Task GoToAsync()
  {
    await _page.GotoAsync(_baseUrl);
  }

  /// <summary>
  /// Waits for the Swagger UI to load.
  /// </summary>
  public async Task WaitForSwaggerToLoadAsync()
  {
    await Expect(SwaggerContainer).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the Swagger UI container is visible.
  /// </summary>
  public async Task<bool> IsSwaggerVisibleAsync()
  {
    return await SwaggerContainer.IsVisibleAsync();
  }

  /// <summary>
  /// Gets the API title text.
  /// </summary>
  public async Task<string?> GetApiTitleAsync()
  {
    await Expect(ApiInfo).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    return await ApiInfo.TextContentAsync();
  }

  /// <summary>
  /// Verifies that the scheme container is visible.
  /// </summary>
  public async Task<bool> HasSchemeContainerAsync()
  {
    return await SchemeContainer.IsVisibleAsync();
  }
}
