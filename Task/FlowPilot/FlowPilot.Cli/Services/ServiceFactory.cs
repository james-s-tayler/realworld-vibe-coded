using FlowPilot.Cli.Linting;
using FlowPilot.Cli.StateTransitions;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Factory for creating service instances with proper dependency injection.
/// </summary>
public class ServiceFactory
{
  private readonly IFileSystemService _fileSystem;
  private readonly TemplateService _templateService;
  private readonly StateParser _stateParser;
  private readonly GitService _gitService;
  private readonly PlanManager _planManager;

  public ServiceFactory(
    IFileSystemService fileSystem,
    TemplateService templateService,
    StateParser stateParser,
    GitService gitService,
    PlanManager planManager)
  {
    _fileSystem = fileSystem;
    _templateService = templateService;
    _stateParser = stateParser;
    _gitService = gitService;
    _planManager = planManager;
  }

  public IEnumerable<ILintingRule> CreateLintingRules()
  {
    return new List<ILintingRule>
    {
      new StateChangesLintingRule(_gitService),
      new StateTransitionsLintingRule(_fileSystem, _stateParser),
      new TemplateChangesLintingRule(_fileSystem, _templateService),
      new ReferencesUrlLintingRule(_fileSystem),
    };
  }

  public IEnumerable<IStateTransition> CreateStateTransitions()
  {
    return new List<IStateTransition>
    {
      new ReferencesTransition(_planManager),
      new SystemAnalysisTransition(_planManager),
      new KeyDecisionsTransition(_planManager),
      new PhaseAnalysisTransition(_planManager),
      new PhaseDetailsTransition(_planManager, _fileSystem, _templateService, _stateParser),
      new PhaseImplementationTransition(_planManager),
    };
  }
}
