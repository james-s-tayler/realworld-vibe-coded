using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that prevents public APIs from exposing EF entities directly.
  /// APIs should use DTOs to decouple transport models from persistence models.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class PublicApisNoEfEntitiesAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV050";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Public APIs must not expose EF entities",
        "Public API method '{0}' exposes EF entity type. Map to DTOs or domain models to prevent coupling transport and persistence models.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ASP.NET controller actions or public application service methods should not return or accept DbContext entities directly. Map to DTOs or domain models to decouple transport layer from persistence.");

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

      // Check if method is in an API endpoint or controller
      var containingType = method.ContainingType;
      if (!IsPublicApiType(containingType))
      {
        return;
      }

      // Check return type for EF entities
      if (IsEfEntityType(method.ReturnType))
      {
        var location = method.Locations.FirstOrDefault();
        if (location != null)
        {
          var diagnostic = Diagnostic.Create(Rule, location, method.Name);
          context.ReportDiagnostic(diagnostic);
        }
      }

      // Check parameters for EF entities
      foreach (var parameter in method.Parameters)
      {
        if (IsEfEntityType(parameter.Type))
        {
          var location = parameter.Locations.FirstOrDefault();
          if (location != null)
          {
            var diagnostic = Diagnostic.Create(Rule, location, method.Name);
            context.ReportDiagnostic(diagnostic);
          }
        }
      }
    }

    private static bool IsPublicApiType(INamedTypeSymbol type)
    {
      // Check if it's a controller (inherits from ControllerBase or has [ApiController])
      var current = type;
      while (current != null)
      {
        if (current.Name == "ControllerBase" || current.Name == "Controller")
        {
          return true;
        }
        current = current.BaseType;
      }

      // Check for API controller attributes
      foreach (var attribute in type.GetAttributes())
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (attributeName == "ApiController" || attributeName == "ApiControllerAttribute")
        {
          return true;
        }
      }

      // Check if it's a FastEndpoints endpoint (implements IEndpoint or inherits from Endpoint)
      current = type;
      while (current != null)
      {
        if (current.Name == "Endpoint" || current.Name.StartsWith("Endpoint"))
        {
          var ns = current.ContainingNamespace?.ToDisplayString() ?? string.Empty;
          if (ns.StartsWith("FastEndpoints"))
          {
            return true;
          }
        }
        current = current.BaseType;
      }

      foreach (var @interface in type.AllInterfaces)
      {
        if (@interface.Name == "IEndpoint")
        {
          var ns = @interface.ContainingNamespace?.ToDisplayString() ?? string.Empty;
          if (ns.StartsWith("FastEndpoints"))
          {
            return true;
          }
        }
      }

      return false;
    }

    private static bool IsEfEntityType(ITypeSymbol type)
    {
      if (type == null)
      {
        return false;
      }

      // Check if type is from Infrastructure.Data or has EF base classes
      var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

      // Check if it's from Infrastructure.Data namespace
      if (namespaceName.Contains(".Infrastructure.Data") ||
          namespaceName.Contains(".Infrastructure.Persistence"))
      {
        // Check if it's an actual entity (not a configuration or migration)
        if (!namespaceName.Contains(".Config") &&
            !namespaceName.Contains(".Migrations") &&
            !namespaceName.Contains(".Queries"))
        {
          // Check if it's a class (not interface)
          if (type.TypeKind == TypeKind.Class)
          {
            return true;
          }
        }
      }

      // Check generic type arguments
      if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
      {
        foreach (var typeArg in namedType.TypeArguments)
        {
          if (IsEfEntityType(typeArg))
          {
            return true;
          }
        }
      }

      return false;
    }
  }
}
