using FlowPilot.Cli.Models;
using FlowPilot.Cli.StateTransitions;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Handles the logic for the 'next' command using discrete state transitions.
/// </summary>
public class NextCommandHandler
{
  private readonly PlanManager _planManager;
  private readonly IEnumerable<IStateTransition> _stateTransitions;

  public NextCommandHandler(
    PlanManager planManager,
    IEnumerable<IStateTransition> stateTransitions)
  {
    _planManager = planManager;
    _stateTransitions = stateTransitions;
  }

  public void Execute(string planName)
  {
    var state = _planManager.GetCurrentState(planName);

    if (!state.IsInitialized)
    {
      Console.WriteLine("Error: Plan not initialized. Run 'flowpilot init' first.");
      Environment.Exit(1);
      return;
    }

    // Build plan context
    var context = new PlanContext
    {
      PlanName = planName,
      State = state,
      PlanDirectory = _planManager.GetPlanDirectory(planName),
      MetaDirectory = _planManager.GetMetaDirectory(planName),
      PlanSubDirectory = _planManager.GetPlanSubDirectory(planName),
      StateFilePath = _planManager.GetStateFilePath(planName),
    };

    // Find and execute the first applicable transition
    var transition = _stateTransitions.FirstOrDefault(t => t.CanTransition(context));

    if (transition != null)
    {
      transition.Execute(context);
    }
    else
    {
      Console.WriteLine("Error: No applicable state transition found.");
      Environment.Exit(1);
    }
  }
}
