using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that prevents setting IncludeEntityObjects = true on AuditDbContext attribute.
  /// When IncludeEntityObjects is true, the entire entity object graph is logged, which
  /// bypasses the [AuditIgnore] attribute and can accidentally log sensitive information
  /// like passwords.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class AuditDbContextIncludeEntityObjectsAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV008";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "IncludeEntityObjects should not be set to true on AuditDbContext attribute",
        "Setting IncludeEntityObjects = true causes the entire entity object graph to be logged, bypassing [AuditIgnore] attributes and potentially logging sensitive information like passwords. Use IncludeEntityObjects = false instead.",
        "Security",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "When IncludeEntityObjects is set to true, the [AuditIgnore] attribute only applies to the ColumnValues section, not the Entity object itself. This can lead to accidentally logging sensitive data. Always use IncludeEntityObjects = false to ensure sensitive properties marked with [AuditIgnore] are completely excluded from audit logs.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      // Register for attribute syntax to check AuditDbContext attributes
      context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
      var attribute = (AttributeSyntax)context.Node;

      // Get the symbol for the attribute
      var attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
      if (attributeSymbol == null)
      {
        return;
      }

      // Check if this is the AuditDbContext attribute
      var attributeClass = attributeSymbol.ContainingType;
      if (attributeClass?.Name != "AuditDbContextAttribute" ||
          attributeClass?.ContainingNamespace?.ToDisplayString() != "Audit.EntityFramework")
      {
        return;
      }

      // Check the attribute arguments for IncludeEntityObjects = true
      if (attribute.ArgumentList == null)
      {
        return;
      }

      foreach (var argument in attribute.ArgumentList.Arguments)
      {
        // Check if this is a named argument with name "IncludeEntityObjects"
        if (argument.NameEquals?.Name?.Identifier.Text == "IncludeEntityObjects")
        {
          // Check if the value is 'true'
          if (argument.Expression is LiteralExpressionSyntax literal &&
              literal.IsKind(SyntaxKind.TrueLiteralExpression))
          {
            var diagnostic = Diagnostic.Create(Rule, argument.GetLocation());
            context.ReportDiagnostic(diagnostic);
          }
        }
      }
    }
  }
}
