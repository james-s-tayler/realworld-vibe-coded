using FlowPilot.Cli.Commands;
using FlowPilot.Cli.Linting;
using FlowPilot.Cli.StateTransitions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Configures dependency injection for FlowPilot services.
/// </summary>
public static class ServiceConfiguration
{
  public static IServiceCollection ConfigureServices(this IServiceCollection services, string currentDirectory)
  {
    // Core services
    services.AddSingleton<IFileSystemService, FileSystemService>();
    services.AddSingleton<TemplateService>();
    services.AddSingleton<StateParser>();
    services.AddSingleton<PlanManager>();
    services.AddSingleton(sp => new GitService(currentDirectory));

    // Linting rules
    services.AddTransient<ILintingRule, StateChangesLintingRule>();
    services.AddTransient<ILintingRule, StateTransitionsLintingRule>();
    services.AddTransient<ILintingRule, PullRequestMergeBoundary>();
    services.AddTransient<ILintingRule, GoalTemplateChangedRule>();
    services.AddTransient<ILintingRule, ReferencesTemplateChangedRule>();
    services.AddTransient<ILintingRule, SystemAnalysisTemplateChangedRule>();
    services.AddTransient<ILintingRule, KeyDecisionsTemplateChangedRule>();
    services.AddTransient<ILintingRule, PhaseAnalysisTemplateChangedRule>();
    services.AddTransient<ILintingRule, PhaseDetailsTemplateChangedRule>();
    services.AddTransient<ILintingRule, ReferencesUrlLintingRule>();

    // State transitions
    services.AddTransient<IStateTransition, ReferencesTransition>();
    services.AddTransient<IStateTransition, SystemAnalysisTransition>();
    services.AddTransient<IStateTransition, KeyDecisionsTransition>();
    services.AddTransient<IStateTransition, PhaseAnalysisTransition>();
    services.AddTransient<IStateTransition, PhaseDetailsTransition>();
    services.AddTransient<IStateTransition, PhaseImplementationTransition>();

    // Command handlers
    services.AddTransient<LintCommandHandler>();
    services.AddTransient<NextCommandHandler>();

    // Commands as keyed services
    services.AddKeyedTransient<ICommand, HelpCommand>("help");
    services.AddKeyedTransient<ICommand, InitCommand>("init");
    services.AddKeyedTransient<ICommand, NewCommandHandler>("new");
    services.AddKeyedTransient<ICommand, LintCommandExecutor>("lint");
    services.AddKeyedTransient<ICommand, NextCommandExecutor>("next");

    return services;
  }
}
