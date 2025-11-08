using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that ensures IEntityTypeConfiguration implementations are in the correct namespace.
  /// This keeps EF mapping centralized and discoverable.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EntityConfigurationLocationAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV040";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "IEntityTypeConfiguration classes must live under Infrastructure.Persistence.Configurations",
        "IEntityTypeConfiguration implementation '{0}' must be in namespace ending with '.Data.Config' or '.Infrastructure.Data.Config' to keep mapping centralized and discoverable",
        "Persistence",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "IEntityTypeConfiguration<T> implementations should be organized in a centralized location under Infrastructure.Persistence.Configurations (or .Data.Config) namespace for discoverability and maintainability.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
      var namedType = (INamedTypeSymbol)context.Symbol;

      // Check if this type implements IEntityTypeConfiguration
      if (!ImplementsIEntityTypeConfiguration(namedType))
      {
        return;
      }

      // Check if the namespace is acceptable
      var namespaceName = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;

      // Allow in .Data.Config or .Infrastructure.Data.Config namespaces
      if (namespaceName.EndsWith(".Data.Config") ||
          namespaceName.EndsWith(".Infrastructure.Data.Config") ||
          namespaceName.EndsWith(".Persistence.Configurations") ||
          namespaceName.EndsWith(".Infrastructure.Persistence.Configurations"))
      {
        return;
      }

      var location = namedType.Locations.FirstOrDefault();
      if (location != null)
      {
        var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool ImplementsIEntityTypeConfiguration(INamedTypeSymbol type)
    {
      foreach (var @interface in type.AllInterfaces)
      {
        if (@interface.Name == "IEntityTypeConfiguration" &&
            @interface.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore") == true)
        {
          return true;
        }
      }
      return false;
    }
  }
}
