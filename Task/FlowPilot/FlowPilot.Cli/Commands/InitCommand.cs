using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the init command that installs FlowPilot into a repository.
/// </summary>
public class InitCommand : ICommand
{
  private readonly IFileSystemService _fileSystem;

  public InitCommand(IFileSystemService fileSystem)
  {
    _fileSystem = fileSystem;
  }

  public string Name => "init";

  public string Description => "Install FlowPilot into the current repository";

  public Task<int> ExecuteAsync(string[] args)
  {
    Console.WriteLine("Installing FlowPilot into repository...");

    var currentDir = _fileSystem.GetCurrentDirectory();

    // Check if already installed
    var githubAgentsDir = Path.Combine(currentDir, ".github", "agents");
    var flowpilotAgentFile = Path.Combine(githubAgentsDir, "flowpilot.agent.md");

    if (_fileSystem.FileExists(flowpilotAgentFile))
    {
      Console.WriteLine("⚠️  FlowPilot appears to already be installed (found .github/agents/flowpilot.agent.md)");
      Console.WriteLine("To reinstall, delete the existing files and run init again.");
      return Task.FromResult(1);
    }

    try
    {
      // Create directories
      var githubInstructionsDir = Path.Combine(currentDir, ".github", "instructions");
      var flowpilotTemplateDir = Path.Combine(currentDir, ".flowpilot", "template");

      _fileSystem.CreateDirectory(githubAgentsDir);
      _fileSystem.CreateDirectory(githubInstructionsDir);
      _fileSystem.CreateDirectory(flowpilotTemplateDir);

      // Copy agent file
      var sourceAgentFile = GetEmbeddedResourcePath("Installation/agents/flowpilot.agent.md");
      _fileSystem.CopyFile(sourceAgentFile, flowpilotAgentFile);

      // Copy instructions file
      var sourceInstructionsFile = GetEmbeddedResourcePath("Installation/instructions/flowpilot-phase-details.instructions.md");
      var targetInstructionsFile = Path.Combine(githubInstructionsDir, "flowpilot-phase-details.instructions.md");
      _fileSystem.CopyFile(sourceInstructionsFile, targetInstructionsFile);

      // Copy template files
      var templateFiles = new[]
      {
        "state.md",
        "goal.md",
        "references.md",
        "system-analysis.md",
        "key-decisions.md",
        "phase-analysis.md",
        "phase-n-details.md",
      };

      foreach (var templateFile in templateFiles)
      {
        var sourceFile = GetEmbeddedResourcePath($"Installation/template/{templateFile}");
        var targetFile = Path.Combine(flowpilotTemplateDir, templateFile);
        _fileSystem.CopyFile(sourceFile, targetFile);
      }

      Console.WriteLine("✓ FlowPilot installed successfully!");
      Console.WriteLine();
      Console.WriteLine("Files installed:");
      Console.WriteLine($"  • .github/agents/flowpilot.agent.md");
      Console.WriteLine($"  • .github/instructions/flowpilot-phase-details.instructions.md");
      Console.WriteLine($"  • .flowpilot/template/ (7 template files)");
      Console.WriteLine();
      Console.WriteLine("Next steps:");
      Console.WriteLine("  1. Commit these files to your repository");
      Console.WriteLine("  2. Run 'flowpilot new <plan-name>' to create your first plan");

      return Task.FromResult(0);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error installing FlowPilot: {ex.Message}");
      return Task.FromResult(1);
    }
  }

  private string GetEmbeddedResourcePath(string relativePath)
  {
    // For now, get from the FlowPilot.Cli project directory
    // In the packaged tool, these will be embedded resources
    var assemblyDir = Path.GetDirectoryName(typeof(InitCommand).Assembly.Location);
    var installationPath = Path.Combine(assemblyDir!, relativePath);

    if (!File.Exists(installationPath))
    {
      // Fallback: try relative to current directory (for development)
      var devPath = Path.Combine(_fileSystem.GetCurrentDirectory(), "Task", "FlowPilot", "FlowPilot.Cli", relativePath);
      if (File.Exists(devPath))
      {
        return devPath;
      }
    }

    return installationPath;
  }
}
