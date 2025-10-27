using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that prevents EF Core types from being referenced outside Infrastructure layer.
  /// This keeps EF details isolated to the persistence layer.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EfCoreTypesOnlyInInfrastructureAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "EF Core types must not be referenced outside Infrastructure.Persistence",
        "EF Core type '{0}' should not be referenced in {1}. Keep EF details (DbContext, DbSet, IEntityTypeConfiguration, Migrations) isolated to Infrastructure layer.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "EF Core types from Microsoft.EntityFrameworkCore.* namespaces should only be used in Infrastructure layer. Wrap with repository/abstraction, move code into Infrastructure, or inject an abstraction.");

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

      // Get the namespace of the type being analyzed
      var containingNamespace = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;

      // Only check types NOT in Infrastructure layer
      if (IsInfrastructureNamespace(containingNamespace))
      {
        return;
      }

      // Check base types
      CheckTypeForEfCore(context, namedType.BaseType, namedType, "base type");

      // Check implemented interfaces
      foreach (var @interface in namedType.Interfaces)
      {
        CheckTypeForEfCore(context, @interface, namedType, "interface");
      }

      // Check all members
      foreach (var member in namedType.GetMembers())
      {
        if (member is IMethodSymbol method)
        {
          // Check return type
          CheckTypeForEfCore(context, method.ReturnType, namedType, "return type");

          // Check parameters
          foreach (var parameter in method.Parameters)
          {
            CheckTypeForEfCore(context, parameter.Type, namedType, "parameter type");
          }
        }
        else if (member is IPropertySymbol property)
        {
          CheckTypeForEfCore(context, property.Type, namedType, "property type");
        }
        else if (member is IFieldSymbol field)
        {
          CheckTypeForEfCore(context, field.Type, namedType, "field type");
        }
      }
    }

    private static void CheckTypeForEfCore(
        SymbolAnalysisContext context,
        ITypeSymbol typeToCheck,
        INamedTypeSymbol declaringType,
        string usageContext)
    {
      if (typeToCheck == null)
      {
        return;
      }

      // Check if the type is from EF Core namespace
      if (IsEfCoreType(typeToCheck))
      {
        var location = declaringType.Locations.FirstOrDefault();
        if (location != null)
        {
          var namespaceName = declaringType.ContainingNamespace?.ToDisplayString() ?? "unknown namespace";
          var diagnostic = Diagnostic.Create(
              Rule,
              location,
              typeToCheck.ToDisplayString(),
              namespaceName);
          context.ReportDiagnostic(diagnostic);
        }
      }

      // Check generic type arguments recursively
      if (typeToCheck is INamedTypeSymbol namedType && namedType.IsGenericType)
      {
        foreach (var typeArg in namedType.TypeArguments)
        {
          CheckTypeForEfCore(context, typeArg, declaringType, usageContext);
        }
      }
    }

    private static bool IsInfrastructureNamespace(string namespaceName)
    {
      return namespaceName.Contains(".Infrastructure") ||
             namespaceName.Contains(".Data") ||
             namespaceName.Contains(".Persistence");
    }

    private static bool IsEfCoreType(ITypeSymbol type)
    {
      if (type == null)
      {
        return false;
      }

      var containingNamespace = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

      // Check for EF Core namespaces
      return containingNamespace.StartsWith("Microsoft.EntityFrameworkCore");
    }
  }
}
