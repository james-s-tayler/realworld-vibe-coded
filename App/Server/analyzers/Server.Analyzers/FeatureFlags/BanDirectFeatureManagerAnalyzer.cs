using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FeatureFlags
{
  /// <summary>
  /// Bans direct injection of IFeatureManager or IVariantFeatureManager.
  /// All feature flag evaluation must go through IFeatureFlagService to ensure
  /// targeting context is applied consistently.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanDirectFeatureManagerAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "FF002";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Direct IFeatureManager injection is banned",
        "Inject IFeatureFlagService instead of {0} to keep feature flag evaluation behind a single abstraction",
        "FeatureFlags",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Direct injection of IFeatureManager or IVariantFeatureManager bypasses the targeting pipeline. Use IFeatureFlagService instead.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var assemblyName = compilationContext.Compilation.AssemblyName ?? "";

        if (assemblyName.Contains("Test"))
        {
          return;
        }

        compilationContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
      });
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
      var namedType = (INamedTypeSymbol)context.Symbol;

      if (namedType.Name == "FeatureFlagService")
      {
        return;
      }

      foreach (var constructor in namedType.Constructors)
      {
        foreach (var parameter in constructor.Parameters)
        {
          if (IsFeatureManagerType(parameter.Type))
          {
            var location = parameter.Locations.FirstOrDefault();
            if (location != null)
            {
              context.ReportDiagnostic(Diagnostic.Create(Rule, location, parameter.Type.Name));
            }
          }
        }
      }

      foreach (var member in namedType.GetMembers())
      {
        if (member is IFieldSymbol field && IsFeatureManagerType(field.Type))
        {
          var location = field.Locations.FirstOrDefault();
          if (location != null)
          {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, field.Type.Name));
          }
        }
      }
    }

    private static bool IsFeatureManagerType(ITypeSymbol type)
    {
      if (IsMatchingType(type))
      {
        return true;
      }

      foreach (var iface in type.AllInterfaces)
      {
        if (IsMatchingType(iface))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsMatchingType(ITypeSymbol type)
    {
      var ns = type.ContainingNamespace?.ToDisplayString();

      if (ns != "Microsoft.FeatureManagement")
      {
        return false;
      }

      return type.Name is "IFeatureManager" or "IVariantFeatureManager";
    }
  }
}
