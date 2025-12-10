namespace FlowPilot.Cli.Models;

/// <summary>
/// Represents the state of a single phase in a FlowPilot plan.
/// </summary>
public class PhaseState
{
  public int PhaseNumber { get; set; }

  public string PhaseName { get; set; } = string.Empty;

  public bool IsComplete { get; set; }
}
