using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that forbids DateTime.Now and DateTimeOffset.Now in LINQ queries.
  /// These should use UTC variants for timezone consistency and proper database translation.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ForbidDateTimeNowInQueriesAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV030";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Forbid DateTime.Now/DateTimeOffset.Now in queries",
        "Use DateTime.UtcNow or DateTimeOffset.UtcNow instead of {0} in LINQ queries for timezone consistency",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "DateTime.Now and DateTimeOffset.Now should not be used in LINQ queries. Use UTC variants (DateTime.UtcNow or DateTimeOffset.UtcNow) for timezone consistency and proper database translation.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
      var memberAccess = (MemberAccessExpressionSyntax)context.Node;

      // Check if accessing .Now property
      if (memberAccess.Name.Identifier.Text != "Now")
      {
        return;
      }

      // Get the symbol info
      var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
      var propertySymbol = symbolInfo.Symbol as IPropertySymbol;

      if (propertySymbol == null)
      {
        return;
      }

      // Check if it's DateTime.Now or DateTimeOffset.Now
      var containingType = propertySymbol.ContainingType;
      if (containingType == null)
      {
        return;
      }

      var isDateTime = containingType.Name == "DateTime" &&
                      containingType.ContainingNamespace?.ToDisplayString() == "System";
      var isDateTimeOffset = containingType.Name == "DateTimeOffset" &&
                            containingType.ContainingNamespace?.ToDisplayString() == "System";

      if (!isDateTime && !isDateTimeOffset)
      {
        return;
      }

      // Check if we're inside a lambda expression (likely part of a LINQ query)
      if (!IsInsideLambdaOrQuery(memberAccess))
      {
        return;
      }

      var typeName = containingType.Name + ".Now";
      var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), typeName);
      context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInsideLambdaOrQuery(SyntaxNode node)
    {
      var current = node.Parent;
      while (current != null)
      {
        // Check for lambda expressions (used in Where, Select, etc.)
        if (current is LambdaExpressionSyntax)
        {
          return true;
        }

        // Check for query expressions (from ... where ... select)
        if (current is QueryExpressionSyntax)
        {
          return true;
        }

        // Check for LINQ method calls on IQueryable
        if (current is InvocationExpressionSyntax invocation)
        {
          var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
          if (memberAccess != null)
          {
            // If this is a LINQ extension method, we're likely in a query context
            var methodName = memberAccess.Name.Identifier.Text;
            var linqMethods = new[]
            {
              "Where", "Select", "SelectMany", "OrderBy", "OrderByDescending",
              "ThenBy", "ThenByDescending", "GroupBy", "Join", "GroupJoin",
              "Take", "Skip", "TakeWhile", "SkipWhile", "First", "FirstOrDefault",
              "Single", "SingleOrDefault", "Last", "LastOrDefault", "Any", "All",
              "Count", "LongCount", "Sum", "Average", "Min", "Max", "Aggregate"
            };

            if (System.Array.IndexOf(linqMethods, methodName) >= 0)
            {
              return true;
            }
          }
        }

        current = current.Parent;
      }

      return false;
    }
  }
}
