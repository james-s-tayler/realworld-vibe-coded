using FlowPilot.Cli.Services;
using Microsoft.Extensions.Logging;

namespace FlowPilot.Cli.Commands;

/// <summary>
/// Handler for the init command that installs FlowPilot into a repository.
/// </summary>
public class InitCommand : ICommand
{
  private readonly IFileSystemService _fileSystem;
  private readonly ILogger<InitCommand> _logger;

  public InitCommand(IFileSystemService fileSystem, ILogger<InitCommand> logger)
  {
    _fileSystem = fileSystem;
    _logger = logger;
  }

  public string Name => "init";

  public string Description => "Install FlowPilot into the current repository";

  public Task<int> ExecuteAsync(string[] args)
  {
    _logger.LogInformation("Installing FlowPilot into repository...");

    var currentDir = _fileSystem.GetCurrentDirectory();

    // Check if already installed
    var githubAgentsDir = Path.Combine(currentDir, ".github", "agents");
    var flowpilotAgentFile = Path.Combine(githubAgentsDir, "flowpilot.agent.md");

    if (_fileSystem.FileExists(flowpilotAgentFile))
    {
      _logger.LogWarning("FlowPilot appears to already be installed (found .github/agents/flowpilot.agent.md)");
      _logger.LogInformation("To reinstall, delete the existing files and run init again.");
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

      _logger.LogInformation("✓ FlowPilot installed successfully!");
      _logger.LogInformation(string.Empty);
      _logger.LogInformation("Files installed:");
      _logger.LogInformation("  • .github/agents/flowpilot.agent.md");
      _logger.LogInformation("  • .github/instructions/flowpilot-phase-details.instructions.md");
      _logger.LogInformation("  • .flowpilot/template/ (7 template files)");
      _logger.LogInformation(string.Empty);
      _logger.LogInformation("Next steps:");
      _logger.LogInformation("  1. Commit these files to your repository");
      _logger.LogInformation("  2. Run 'flowpilot new <plan-name>' to create your first plan");

      return Task.FromResult(0);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error installing FlowPilot");
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
