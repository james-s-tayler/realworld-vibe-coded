using System.Text.RegularExpressions;
using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Service for parsing phase information from phase-analysis.md files.
/// </summary>
public partial class PhaseAnalysisParser
{
  private readonly IFileSystemService _fileSystem;

  public PhaseAnalysisParser(IFileSystemService fileSystem)
  {
    _fileSystem = fileSystem;
  }

  /// <summary>
  /// Parses phase information from a phase-analysis.md file.
  /// </summary>
  public List<PhaseInfo> ParsePhases(string phaseAnalysisPath)
  {
    var phases = new List<PhaseInfo>();

    if (!_fileSystem.FileExists(phaseAnalysisPath))
    {
      return phases;
    }

    var content = _fileSystem.ReadAllText(phaseAnalysisPath);
    var lines = content.Split('\n');

    PhaseInfo? currentPhase = null;

    foreach (var line in lines)
    {
      var trimmedLine = line.Trim();

      // Check for phase header: ### phase_N
      if (trimmedLine.StartsWith("### phase_", StringComparison.Ordinal))
      {
        // Save previous phase if exists
        if (currentPhase != null)
        {
          phases.Add(currentPhase);
        }

        // Extract phase number
        var phaseName = trimmedLine.Substring(4).Trim(); // Remove "### "
        var match = PhaseNumberRegex().Match(phaseName);

        if (match.Success && int.TryParse(match.Groups[1].Value, out int phaseNumber))
        {
          currentPhase = new PhaseInfo
          {
            PhaseNumber = phaseNumber,
            PhaseName = phaseName,
            IsPullRequestBoundary = false, // Default to false
          };
        }
      }

      // Check for PR Boundary marker
      else if (currentPhase != null && trimmedLine.StartsWith("**PR Boundary**:", StringComparison.OrdinalIgnoreCase))
      {
        var value = trimmedLine.Substring("**PR Boundary**:".Length).Trim().ToLowerInvariant();
        currentPhase.IsPullRequestBoundary = value == "yes" || value == "true";
      }
    }

    // Add the last phase
    if (currentPhase != null)
    {
      phases.Add(currentPhase);
    }

    return phases;
  }

  [GeneratedRegex(@"phase_(\d+)")]
  private static partial Regex PhaseNumberRegex();
}
