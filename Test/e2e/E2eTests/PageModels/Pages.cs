using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Static API for accessing page object instances.
/// Provides centralized access to all page models for use in tests.
/// </summary>
public static class Pages
{
  private static IPage? _page;
  private static string? _baseUrl;

  /// <summary>
  /// Initialize the Pages API with a Playwright page and base URL.
  /// This must be called before accessing any page properties.
  /// </summary>
  public static void Initialize(IPage page, string baseUrl)
  {
    _page = page;
    _baseUrl = baseUrl;
  }

  /// <summary>
  /// Gets the Home page instance.
  /// </summary>
  public static HomePage HomePage
  {
    get
    {
      EnsureInitialized();
      return new HomePage(_page!, _baseUrl!);
    }
  }

  /// <summary>
  /// Gets the Article page instance.
  /// </summary>
  public static ArticlePage ArticlePage
  {
    get
    {
      EnsureInitialized();
      return new ArticlePage(_page!, $"{_baseUrl}/article");
    }
  }

  /// <summary>
  /// Gets the Editor page instance.
  /// </summary>
  public static EditorPage EditorPage
  {
    get
    {
      EnsureInitialized();
      return new EditorPage(_page!, $"{_baseUrl}/editor");
    }
  }

  /// <summary>
  /// Gets the Login page instance.
  /// </summary>
  public static LoginPage LoginPage
  {
    get
    {
      EnsureInitialized();
      return new LoginPage(_page!, $"{_baseUrl}/login");
    }
  }

  /// <summary>
  /// Gets the Register page instance.
  /// </summary>
  public static RegisterPage RegisterPage
  {
    get
    {
      EnsureInitialized();
      return new RegisterPage(_page!, $"{_baseUrl}/register");
    }
  }

  /// <summary>
  /// Gets the Profile page instance.
  /// </summary>
  public static ProfilePage ProfilePage
  {
    get
    {
      EnsureInitialized();
      return new ProfilePage(_page!, $"{_baseUrl}/profile");
    }
  }

  /// <summary>
  /// Gets the Settings page instance.
  /// </summary>
  public static SettingsPage SettingsPage
  {
    get
    {
      EnsureInitialized();
      return new SettingsPage(_page!, $"{_baseUrl}/settings");
    }
  }

  /// <summary>
  /// Gets the Swagger page instance.
  /// </summary>
  public static SwaggerPage SwaggerPage
  {
    get
    {
      EnsureInitialized();
      return new SwaggerPage(_page!, $"{_baseUrl}/swagger/index.html");
    }
  }

  private static void EnsureInitialized()
  {
    if (_page == null || _baseUrl == null)
    {
      throw new InvalidOperationException(
        "Pages API has not been initialized. Call Pages.Initialize(page, baseUrl) before accessing page properties.");
    }
  }
}
