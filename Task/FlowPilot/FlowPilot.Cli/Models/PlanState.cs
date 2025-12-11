namespace FlowPilot.Cli.Models;

/// <summary>
/// Represents the current state of a FlowPilot plan.
/// </summary>
public class PlanState
{
  public bool IsInitialized { get; set; }

  public bool HasReferences { get; set; }

  public bool HasSystemAnalysis { get; set; }

  public bool HasKeyDecisions { get; set; }

  public bool HasPhaseAnalysis { get; set; }

  public bool HasPhaseDetails { get; set; }

  public List<PhaseState> Phases { get; set; } = new List<PhaseState>();

  public string? CurrentPhase { get; set; }
}
