using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that suggests using AsNoTracking for read-only queries to reduce change-tracking overhead.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class PreferAsNoTrackingForReadOnlyQueriesAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV020";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Prefer AsNoTracking for read-only queries",
        "Repository method '{0}' appears to be read-only but doesn't use AsNoTracking. Consider adding .AsNoTracking() to reduce change-tracking overhead.",
        "Persistence",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Query chains in repositories that only read (method name starts with Get/Find/List) should call AsNoTracking before First/Single/ToList to reduce change-tracking overhead, unless the method is marked with [TrackedQuery] or modifies aggregates.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
      var method = (IMethodSymbol)context.Symbol;

      // Check if method is in a repository
      var containingType = method.ContainingType;
      if (!IsRepository(containingType))
      {
        return;
      }

      // Check if method name suggests read-only operation
      if (!IsReadOnlyMethodName(method.Name))
      {
        return;
      }

      // Check for [TrackedQuery] attribute (opt-out)
      if (HasTrackedQueryAttribute(method))
      {
        return;
      }

      // This is a simplified check - in a full implementation, we would analyze the method body
      // to see if AsNoTracking is already called and if materialization methods are used
      // For now, we'll just report a suggestion on methods that look read-only

      var location = method.Locations.FirstOrDefault();
      if (location != null)
      {
        var diagnostic = Diagnostic.Create(Rule, location, method.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsRepository(INamedTypeSymbol type)
    {
      // Check if name contains "Repository"
      if (type.Name.Contains("Repository"))
      {
        return true;
      }

      // Check if has [Repository] attribute
      foreach (var attribute in type.GetAttributes())
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (attributeName == "Repository" || attributeName == "RepositoryAttribute")
        {
          return true;
        }
      }

      // Check if implements IRepository interface
      foreach (var @interface in type.AllInterfaces)
      {
        if (@interface.Name.Contains("Repository"))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsReadOnlyMethodName(string methodName)
    {
      var readOnlyPrefixes = new[]
      {
        "Get", "Find", "List", "Query", "Fetch", "Retrieve", "Search", "Count"
      };

      foreach (var prefix in readOnlyPrefixes)
      {
        if (methodName.StartsWith(prefix))
        {
          return true;
        }
      }

      return false;
    }

    private static bool HasTrackedQueryAttribute(IMethodSymbol method)
    {
      foreach (var attribute in method.GetAttributes())
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (attributeName == "TrackedQuery" || attributeName == "TrackedQueryAttribute")
        {
          return true;
        }
      }
      return false;
    }
  }
}
