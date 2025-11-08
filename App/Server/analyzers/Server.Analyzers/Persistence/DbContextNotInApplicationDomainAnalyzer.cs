using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that prevents DbContext from appearing in the public surface of Application/Domain layers.
  /// DbContext should be isolated to Infrastructure layer behind abstractions.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class DbContextNotInApplicationDomainAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV002";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "DbContext must not appear in public surface of Application/Domain",
        "DbContext type appears in {0} '{1}'. Prevent leaking infrastructure into higher layers by using repository or unit-of-work abstractions instead.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "DbContext, DbSet, and other EF types should not appear as method parameters, return types, fields, properties, or generic type arguments in Domain or Application layers. Use repository or unit-of-work abstractions to decouple from infrastructure.");

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

      // Check all members
      foreach (var member in namedType.GetMembers())
      {
        // Skip non-public members
        if (member.DeclaredAccessibility != Accessibility.Public &&
            member.DeclaredAccessibility != Accessibility.Internal)
        {
          continue;
        }

        if (member is IMethodSymbol method)
        {
          // Check return type
          if (IsDbContextOrEfType(method.ReturnType))
          {
            ReportDiagnostic(context, member, "return type", method.Name);
          }

          // Check parameters
          foreach (var parameter in method.Parameters)
          {
            if (IsDbContextOrEfType(parameter.Type))
            {
              ReportDiagnostic(context, parameter, "parameter", method.Name);
            }
          }
        }
        else if (member is IPropertySymbol property)
        {
          if (IsDbContextOrEfType(property.Type))
          {
            ReportDiagnostic(context, member, "property", property.Name);
          }
        }
        else if (member is IFieldSymbol field)
        {
          if (IsDbContextOrEfType(field.Type))
          {
            ReportDiagnostic(context, member, "field", field.Name);
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

    private static bool IsDbContextOrEfType(ITypeSymbol type)
    {
      if (type == null)
      {
        return false;
      }

      // Check if it's DbContext or derives from it
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

      // Check for DbSet
      if (type.Name == "DbSet" &&
          type.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore") == true)
      {
        return true;
      }

      // Check generic type arguments
      if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
      {
        foreach (var typeArg in namedType.TypeArguments)
        {
          if (IsDbContextOrEfType(typeArg))
          {
            return true;
          }
        }
      }

      return false;
    }

    private static void ReportDiagnostic(SymbolAnalysisContext context, ISymbol symbol, string memberType, string memberName)
    {
      var location = symbol.Locations.FirstOrDefault();
      if (location != null)
      {
        var diagnostic = Diagnostic.Create(Rule, location, memberType, memberName);
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
