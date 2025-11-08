using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that detects EF mapping attributes on domain entities.
  /// Domain entities should remain persistence-agnostic; use Fluent API in Infrastructure instead.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class DomainEntitiesNoEfAttributesAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV013";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Domain entities must not carry EF mapping attributes",
        "Remove EF mapping attribute '{0}' from domain entity. Use Fluent API in IEntityTypeConfiguration instead to keep domain persistence-agnostic.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Domain entities should not have EF mapping attributes like [Table], [Key], [ForeignKey], [Owned], [Index]. Move mapping configuration to Fluent API in Infrastructure via IEntityTypeConfiguration<T>.");

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

      // Check if this type is in a Domain/Core namespace (not Infrastructure or SharedKernel)
      var namespaceName = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;

      // Only check types in Domain/Core namespaces, excluding SharedKernel (framework component)
      if ((!namespaceName.Contains(".Core") && !namespaceName.Contains(".Domain")) ||
          namespaceName.Contains(".SharedKernel"))
      {
        return;
      }

      // Skip if type has [PersistenceAware] attribute (explicit opt-out)
      if (HasAttribute(namedType, "PersistenceAware", "PersistenceAwareAttribute"))
      {
        return;
      }

      // Check for banned EF attributes on the type itself
      CheckForBannedAttributes(context, namedType.GetAttributes(), namedType.Locations.FirstOrDefault());

      // Check properties for banned attributes
      foreach (var member in namedType.GetMembers())
      {
        if (member is IPropertySymbol property)
        {
          CheckForBannedAttributes(context, property.GetAttributes(), property.Locations.FirstOrDefault());
        }
      }
    }

    private static void CheckForBannedAttributes(
        SymbolAnalysisContext context,
        ImmutableArray<AttributeData> attributes,
        Location location)
    {
      if (location == null)
      {
        return;
      }

      var bannedAttributes = new[]
      {
        "Table", "TableAttribute",
        "Key", "KeyAttribute",
        "ForeignKey", "ForeignKeyAttribute",
        "Owned", "OwnedAttribute",
        "Index", "IndexAttribute",
        "Column", "ColumnAttribute",
        "Required", "RequiredAttribute",
        "MaxLength", "MaxLengthAttribute",
        "StringLength", "StringLengthAttribute",
        "NotMapped", "NotMappedAttribute",
        "InverseProperty", "InversePropertyAttribute",
        "ConcurrencyCheck", "ConcurrencyCheckAttribute",
        "Timestamp", "TimestampAttribute"
      };

      foreach (var attribute in attributes)
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;

        if (System.Array.IndexOf(bannedAttributes, attributeName) >= 0)
        {
          // Verify it's from the EF namespace
          var attributeNamespace = attribute.AttributeClass?.ContainingNamespace?.ToDisplayString() ?? string.Empty;
          if (attributeNamespace.StartsWith("System.ComponentModel.DataAnnotations") ||
              attributeNamespace.StartsWith("Microsoft.EntityFrameworkCore"))
          {
            var diagnostic = Diagnostic.Create(Rule, location, attributeName);
            context.ReportDiagnostic(diagnostic);
          }
        }
      }
    }

    private static bool HasAttribute(INamedTypeSymbol type, params string[] attributeNames)
    {
      foreach (var attribute in type.GetAttributes())
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (System.Array.IndexOf(attributeNames, attributeName) >= 0)
        {
          return true;
        }
      }
      return false;
    }
  }
}
