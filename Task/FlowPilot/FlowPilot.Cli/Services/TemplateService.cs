namespace FlowPilot.Cli.Services;

/// <summary>
/// Service for reading template files from the repository's .flowpilot/template directory.
/// </summary>
public class TemplateService
{
  private readonly string _templateDirectory;

  public TemplateService()
  {
    // Template directory is expected to be at .flowpilot/template in the repository root
    var currentDir = Directory.GetCurrentDirectory();
    _templateDirectory = Path.Combine(currentDir, ".flowpilot", "template");
  }

  public string ReadTemplate(string templateName)
  {
    var templatePath = Path.Combine(_templateDirectory, templateName);

    if (!File.Exists(templatePath))
    {
      throw new FileNotFoundException($"Template '{templateName}' not found at {templatePath}. Make sure FlowPilot is installed (run 'flowpilot init').");
    }

    return File.ReadAllText(templatePath);
  }

  public List<string> ListTemplates()
  {
    if (!Directory.Exists(_templateDirectory))
    {
      return new List<string>();
    }

    return Directory.GetFiles(_templateDirectory, "*.md")
      .Select(Path.GetFileName)
      .Where(f => f != null)
      .Select(f => f!)
      .ToList();
  }
}
