namespace FlowPilot.Cli.Models;

/// <summary>
/// Represents a single item in the state.md checklist.
/// </summary>
public class StateChecklistItem
{
  public string Identifier { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public bool IsChecked { get; set; }

  public int? PhaseNumber { get; set; }

  public bool IsPullRequestBoundary { get; set; }
}
