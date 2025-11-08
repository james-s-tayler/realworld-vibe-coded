using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that detects repository interfaces or classes exposing IQueryable or EF types.
  /// Repositories should return materialized collections or domain types, not query providers.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RepositoriesNoIQueryableAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV010";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Repositories must not expose IQueryable or EF types",
        "Repository {0} exposes IQueryable or EF-specific type. Return IEnumerable, IAsyncEnumerable, or concrete domain types instead to avoid leaking query providers across boundaries.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Repository interfaces and their public methods should not return IQueryable<T>, DbSet<T>, or accept IQueryable parameters. This avoids leaking EF-specific query providers and concerns across architectural boundaries. Use IEnumerable<T>, IAsyncEnumerable<T>, or concrete DTO/domain types instead.");

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

      // Check if this is a repository class or interface
      if (!IsRepository(namedType))
      {
        return;
      }

      // Check all public methods and properties
      foreach (var member in namedType.GetMembers())
      {
        if (member.DeclaredAccessibility != Accessibility.Public)
        {
          continue;
        }

        if (member is IMethodSymbol method)
        {
          // Check return type
          if (HasForbiddenType(method.ReturnType))
          {
            var location = member.Locations.FirstOrDefault();
            if (location != null)
            {
              var diagnostic = Diagnostic.Create(Rule, location, member.Name);
              context.ReportDiagnostic(diagnostic);
            }
          }

          // Check parameters
          foreach (var parameter in method.Parameters)
          {
            if (HasForbiddenType(parameter.Type))
            {
              var location = parameter.Locations.FirstOrDefault();
              if (location != null)
              {
                var diagnostic = Diagnostic.Create(Rule, location, member.Name);
                context.ReportDiagnostic(diagnostic);
              }
            }
          }
        }
        else if (member is IPropertySymbol property)
        {
          if (HasForbiddenType(property.Type))
          {
            var location = member.Locations.FirstOrDefault();
            if (location != null)
            {
              var diagnostic = Diagnostic.Create(Rule, location, member.Name);
              context.ReportDiagnostic(diagnostic);
            }
          }
        }
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

    private static bool HasForbiddenType(ITypeSymbol type)
    {
      if (type == null)
      {
        return false;
      }

      // Unwrap generic types
      var originalType = type.OriginalDefinition ?? type;
      var typeName = originalType.Name;

      // Check for IQueryable
      if (typeName == "IQueryable")
      {
        return true;
      }

      // Check for DbSet
      if (typeName == "DbSet" &&
          originalType.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore") == true)
      {
        return true;
      }

      // Check if type implements IQueryable
      if (type is INamedTypeSymbol namedType)
      {
        foreach (var @interface in namedType.AllInterfaces)
        {
          if (@interface.Name == "IQueryable")
          {
            return true;
          }
        }

        // Check generic type argument recursively
        if (namedType.IsGenericType)
        {
          foreach (var typeArg in namedType.TypeArguments)
          {
            if (HasForbiddenType(typeArg))
            {
              return true;
            }
          }
        }
      }

      return false;
    }
  }
}
