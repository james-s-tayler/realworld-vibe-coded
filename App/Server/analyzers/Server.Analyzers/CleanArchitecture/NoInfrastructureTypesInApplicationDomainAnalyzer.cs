using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.CleanArchitecture
{
  /// <summary>
  /// Analyzer that prevents Infrastructure types from being injected into Application/Domain layers.
  /// These layers should depend on abstractions, not concrete infrastructure implementations.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class NoInfrastructureTypesInApplicationDomainAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV051";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Infrastructure types must not be injected into Application/Domain",
        "Infrastructure type '{0}' is injected into Application/Domain layer. Inject IUnitOfWork or repository interfaces instead to depend on abstractions.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Constructor parameters or fields in Application/Domain should not use concrete infrastructure types like AppDbContext, DbContext, or EF-specific helpers. Inject abstractions like IUnitOfWork or repository interfaces instead.");

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

      // Only check types in Application/Domain/Core/UseCases layers
      var namespaceName = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
      if (!IsApplicationOrDomainNamespace(namespaceName))
      {
        return;
      }

      // Check constructor parameters
      foreach (var constructor in namedType.Constructors)
      {
        foreach (var parameter in constructor.Parameters)
        {
          if (IsInfrastructureType(parameter.Type))
          {
            var location = parameter.Locations.FirstOrDefault();
            if (location != null)
            {
              var diagnostic = Diagnostic.Create(Rule, location, parameter.Type.Name);
              context.ReportDiagnostic(diagnostic);
            }
          }
        }
      }

      // Check fields
      foreach (var member in namedType.GetMembers())
      {
        if (member is IFieldSymbol field)
        {
          if (IsInfrastructureType(field.Type))
          {
            var location = field.Locations.FirstOrDefault();
            if (location != null)
            {
              var diagnostic = Diagnostic.Create(Rule, location, field.Type.Name);
              context.ReportDiagnostic(diagnostic);
            }
          }
        }
      }
    }

    private static bool IsApplicationOrDomainNamespace(string namespaceName)
    {
      return namespaceName.Contains(".Core") ||
             namespaceName.Contains(".Domain") ||
             namespaceName.Contains(".Application") ||
             namespaceName.Contains(".UseCases");
    }

    private static bool IsInfrastructureType(ITypeSymbol type)
    {
      if (type == null)
      {
        return false;
      }

      var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

      // Check if type is from Infrastructure namespace (but not an abstraction)
      if (namespaceName.Contains(".Infrastructure"))
      {
        // Allow interfaces (abstractions are okay)
        if (type.TypeKind == TypeKind.Interface)
        {
          return false;
        }

        // Check if it's a concrete class
        if (type.TypeKind == TypeKind.Class)
        {
          return true;
        }
      }

      // Check for DbContext types
      var current = type as INamedTypeSymbol;
      while (current != null)
      {
        if (current.Name == "DbContext" &&
            current.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore") == true)
        {
          return true;
        }
        current = current.BaseType;
      }

      return false;
    }
  }
}
