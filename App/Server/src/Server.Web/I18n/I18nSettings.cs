namespace Server.Web.I18n;

public class I18nSettings
{
  public const string SectionName = "I18n";

  public string DefaultLanguage { get; set; } = "en";

  public string[] SupportedLanguages { get; set; } = ["en", "ja"];
}
