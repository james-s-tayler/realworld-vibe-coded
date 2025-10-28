using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that ensures repository async methods support CancellationToken for cooperative cancellation.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RepositoryAsyncMethodsCancellationTokenAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV011";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Repository methods must support CancellationToken",
        "Async repository method '{0}' should accept a CancellationToken parameter for cooperative cancellation of I/O-bound operations",
        "Persistence",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Public async methods in repositories that perform EF async calls should accept a CancellationToken parameter and pass it through to support cooperative cancellation. This is critical for I/O-bound operations.");

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

      // Only check public methods
      if (method.DeclaredAccessibility != Accessibility.Public)
      {
        return;
      }

      // Only check async methods (returns Task or Task<T>)
      if (!IsAsyncMethod(method))
      {
        return;
      }

      // Check if the containing type is a repository
      var containingType = method.ContainingType;
      if (!IsRepository(containingType))
      {
        return;
      }

      // Check if method has [AllowNoCancellationToken] attribute (explicit opt-out)
      if (HasAllowNoCancellationTokenAttribute(method))
      {
        return;
      }

      // Check if method already has a CancellationToken parameter
      if (HasCancellationTokenParameter(method))
      {
        return;
      }

      var location = method.Locations.FirstOrDefault();
      if (location != null)
      {
        var diagnostic = Diagnostic.Create(Rule, location, method.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsAsyncMethod(IMethodSymbol method)
    {
      var returnType = method.ReturnType;
      if (returnType == null)
      {
        return false;
      }

      var typeName = returnType.Name;
      return typeName == "Task" || typeName == "ValueTask";
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

    private static bool HasCancellationTokenParameter(IMethodSymbol method)
    {
      foreach (var parameter in method.Parameters)
      {
        if (parameter.Type.Name == "CancellationToken" &&
            parameter.Type.ContainingNamespace?.ToDisplayString() == "System.Threading")
        {
          return true;
        }
      }
      return false;
    }

    private static bool HasAllowNoCancellationTokenAttribute(IMethodSymbol method)
    {
      foreach (var attribute in method.GetAttributes())
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (attributeName == "AllowNoCancellationToken" ||
            attributeName == "AllowNoCancellationTokenAttribute")
        {
          return true;
        }
      }
      return false;
    }
  }
}
