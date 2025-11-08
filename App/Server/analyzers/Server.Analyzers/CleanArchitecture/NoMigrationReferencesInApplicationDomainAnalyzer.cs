using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.CleanArchitecture
{
  /// <summary>
  /// Analyzer that prevents migration classes from being referenced outside Infrastructure layer.
  /// Migrations are implementation details and should not leak into Application/Domain.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class NoMigrationReferencesInApplicationDomainAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV041";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Migration classes must not be referenced by Application/Domain",
        "Migration type '{0}' should not be referenced in Application/Domain layers. Migrations are infrastructure implementation details and must remain isolated.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using statements or symbol references to EF Core Migration classes in Application/Domain layers violate layer separation. Keep migration-only types isolated to Infrastructure.");

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

      // Check base types
      if (IsMigrationType(namedType.BaseType))
      {
        ReportDiagnostic(context, namedType, namedType.BaseType);
      }

      // Check all members for migration type usage
      foreach (var member in namedType.GetMembers())
      {
        if (member is IMethodSymbol method)
        {
          // Check return type
          if (IsMigrationType(method.ReturnType))
          {
            ReportDiagnostic(context, member, method.ReturnType);
          }

          // Check parameters
          foreach (var parameter in method.Parameters)
          {
            if (IsMigrationType(parameter.Type))
            {
              ReportDiagnostic(context, parameter, parameter.Type);
            }
          }
        }
        else if (member is IPropertySymbol property)
        {
          if (IsMigrationType(property.Type))
          {
            ReportDiagnostic(context, member, property.Type);
          }
        }
        else if (member is IFieldSymbol field)
        {
          if (IsMigrationType(field.Type))
          {
            ReportDiagnostic(context, member, field.Type);
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

    private static bool IsMigrationType(ITypeSymbol type)
    {
      if (type == null)
      {
        return false;
      }

      // Check if type inherits from Migration
      var current = type as INamedTypeSymbol;
      while (current != null)
      {
        if (current.Name == "Migration" &&
            current.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore.Migrations") == true)
        {
          return true;
        }
        current = current.BaseType;
      }

      // Check if type is in a Migrations namespace
      var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
      if (namespaceName.Contains(".Migrations") || namespaceName.Contains(".Data.Migrations"))
      {
        return true;
      }

      return false;
    }

    private static void ReportDiagnostic(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol migrationType)
    {
      var location = symbol.Locations.FirstOrDefault();
      if (location != null)
      {
        var diagnostic = Diagnostic.Create(Rule, location, migrationType.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
