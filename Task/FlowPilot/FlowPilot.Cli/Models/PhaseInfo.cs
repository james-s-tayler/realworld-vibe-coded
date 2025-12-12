namespace FlowPilot.Cli.Models;

/// <summary>
/// Represents parsed phase information from phase-analysis.md.
/// </summary>
public class PhaseInfo
{
  public int PhaseNumber { get; set; }

  public string PhaseName { get; set; } = string.Empty;

  public bool IsPullRequestBoundary { get; set; }
}
