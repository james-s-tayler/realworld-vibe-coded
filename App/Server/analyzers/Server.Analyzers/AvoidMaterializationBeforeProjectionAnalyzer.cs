using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that detects materialization (ToList/AsEnumerable) before projection or filtering.
  /// This causes unnecessary data to be fetched from the database.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class AvoidMaterializationBeforeProjectionAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV021";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Avoid materializing before projection or filtering",
        "Materialization method '{0}' is followed by '{1}'. Reorder to perform {1} before materialization to push computation to the database and improve performance.",
        "Persistence",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ToList/ToListAsync/AsEnumerable should not be followed by Select/Where/OrderBy/GroupBy. Reorder operations to perform projection and filtering before materialization to leverage database query optimization.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
      var invocation = (InvocationExpressionSyntax)context.Node;

      // Check if this is a chained method call (memberAccess.Method())
      if (invocation.Expression is not MemberAccessExpressionSyntax currentMemberAccess)
      {
        return;
      }

      var currentMethod = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
      if (currentMethod == null)
      {
        return;
      }

      // Check if current method is a projection/filtering method
      var projectionMethods = new[]
      {
        "Select", "Where", "OrderBy", "OrderByDescending",
        "ThenBy", "ThenByDescending", "GroupBy"
      };

      if (System.Array.IndexOf(projectionMethods, currentMethod.Name) < 0)
      {
        return;
      }

      // Walk back to find if there's a materialization method before this
      var previousExpression = currentMemberAccess.Expression;

      if (previousExpression is InvocationExpressionSyntax previousInvocation)
      {
        var previousMethod = context.SemanticModel.GetSymbolInfo(previousInvocation).Symbol as IMethodSymbol;
        if (previousMethod != null && IsMaterializationMethod(previousMethod.Name))
        {
          var diagnostic = Diagnostic.Create(
              Rule,
              previousInvocation.GetLocation(),
              previousMethod.Name,
              currentMethod.Name);
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static bool IsMaterializationMethod(string methodName)
    {
      var materializationMethods = new[]
      {
        "ToList",
        "ToListAsync",
        "AsEnumerable",
        "ToArray",
        "ToArrayAsync"
      };

      return System.Array.IndexOf(materializationMethods, methodName) >= 0;
    }
  }
}
