using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Transition to the references phase.
/// </summary>
public class ReferencesTransition : IStateTransition
{
  private readonly PlanManager _planManager;

  public ReferencesTransition(PlanManager planManager)
  {
    _planManager = planManager;
  }

  public bool CanTransition(PlanContext context)
  {
    return !context.State.HasReferences;
  }

  public void Execute(PlanContext context)
  {
    _planManager.UpdateStateChecklist(context.PlanName, "references", true);
    _planManager.CopyTemplateToMeta(context.PlanName, "references.md");

    Console.WriteLine("✓ Advanced to [references] phase");
    Console.WriteLine();
    Console.WriteLine("Instructions:");
    Console.WriteLine($"Update .flowpilot/plans/{context.PlanName}/meta/references.md");
    Console.WriteLine("Use the mslearn MCP server and web search to conduct thorough research");
    Console.WriteLine("based on the goal stated in goal.md. Document your findings in references.md");
    Console.WriteLine("to aid the implementation plan.");
  }
}
