namespace FlowPilot.Cli.Models;

/// <summary>
/// Context object containing all information about a plan.
/// </summary>
public class PlanContext
{
  public string PlanName { get; set; } = string.Empty;

  public PlanState State { get; set; } = new PlanState();

  public string PlanDirectory { get; set; } = string.Empty;

  public string MetaDirectory { get; set; } = string.Empty;

  public string PlanSubDirectory { get; set; } = string.Empty;

  public string StateFilePath { get; set; } = string.Empty;
}
