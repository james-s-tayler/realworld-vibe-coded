using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.StateTransitions;

/// <summary>
/// Interface for discrete state transitions.
/// </summary>
public interface IStateTransition
{
  /// <summary>
  /// Check if this transition can be applied to the current state.
  /// </summary>
  bool CanTransition(PlanContext context);

  /// <summary>
  /// Execute the state transition.
  /// </summary>
  void Execute(PlanContext context);
}
