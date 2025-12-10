using System.Reflection;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Service for reading embedded template resources.
/// </summary>
public class TemplateService
{
  private readonly Assembly _assembly;

  public TemplateService()
  {
    _assembly = Assembly.GetExecutingAssembly();
  }

  public string ReadTemplate(string templateName)
  {
    var resourceName = $"FlowPilot.Cli.Templates.{templateName}";
    using var stream = _assembly.GetManifestResourceStream(resourceName);

    if (stream == null)
    {
      throw new FileNotFoundException($"Template '{templateName}' not found in embedded resources.");
    }

    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }

  public List<string> ListTemplates()
  {
    var resourceNames = _assembly.GetManifestResourceNames()
      .Where(n => n.StartsWith("FlowPilot.Cli.Templates.", StringComparison.Ordinal))
      .Select(n => n.Substring("FlowPilot.Cli.Templates.".Length))
      .ToList();

    return resourceNames;
  }
}
