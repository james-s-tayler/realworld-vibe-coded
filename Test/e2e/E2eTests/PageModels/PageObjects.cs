using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// API for accessing page object instances.
/// Provides centralized access to all page models for use in tests.
/// </summary>
public class PageObjects
{
  private readonly IPage _page;
  private readonly string _baseUrl;

  /// <summary>
  /// Initialize the Pages API with a Playwright page and base URL.
  /// </summary>
  public PageObjects(IPage page, string baseUrl)
  {
    _page = page;
    _baseUrl = baseUrl;
  }

  /// <summary>
  /// Gets the Home page instance.
  /// </summary>
  public HomePage HomePage => new(_page, _baseUrl);

  /// <summary>
  /// Gets the Article page instance.
  /// </summary>
  public ArticlePage ArticlePage => new(_page, $"{_baseUrl}/article");

  /// <summary>
  /// Gets the Editor page instance.
  /// </summary>
  public EditorPage EditorPage => new(_page, $"{_baseUrl}/editor");

  /// <summary>
  /// Gets the Login page instance.
  /// </summary>
  public LoginPage LoginPage => new(_page, $"{_baseUrl}/login");

  /// <summary>
  /// Gets the Register page instance.
  /// </summary>
  public RegisterPage RegisterPage => new(_page, $"{_baseUrl}/register");

  /// <summary>
  /// Gets the Profile page instance.
  /// </summary>
  public ProfilePage ProfilePage => new(_page, $"{_baseUrl}/profile");

  /// <summary>
  /// Gets the Settings page instance.
  /// </summary>
  public SettingsPage SettingsPage => new(_page, $"{_baseUrl}/settings");

  /// <summary>
  /// Gets the Swagger page instance.
  /// </summary>
  public SwaggerPage SwaggerPage => new(_page, $"{_baseUrl}/swagger/index.html");
}
