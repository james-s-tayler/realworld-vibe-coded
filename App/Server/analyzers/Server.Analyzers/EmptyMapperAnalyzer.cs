using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EmptyMapperAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV002";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Empty mapper passed to ResultMapperAsync",
        "Use 'ResultValueAsync' instead of 'ResultMapperAsync' with an empty mapper. Empty mappers (e.g., '_ => new {{ }}') serve no purpose and should be replaced with 'ResultValueAsync'.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ResultMapperAsync should only be used when actually mapping/transforming the result value. If no transformation is needed, use ResultValueAsync instead.");

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

      // Check if this is a call to ResultMapperAsync
      if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
          memberAccess.Name.Identifier.Text == "ResultMapperAsync")
      {
        // Get the arguments
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count >= 2)
        {
          // The second argument should be the mapper function
          var mapperArgument = arguments[1].Expression;

          // Check if it's a lambda expression
          if (mapperArgument is SimpleLambdaExpressionSyntax simpleLambda)
          {
            if (IsEmptyObjectCreation(simpleLambda.Body))
            {
              var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
              context.ReportDiagnostic(diagnostic);
            }
          }
          else if (mapperArgument is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
          {
            if (IsEmptyObjectCreation(parenthesizedLambda.Body))
            {
              var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
              context.ReportDiagnostic(diagnostic);
            }
          }
        }
      }
    }

    private static bool IsEmptyObjectCreation(SyntaxNode node)
    {
      if (node == null)
      {
        return false;
      }

      // Check if it's directly an empty object creation: new { }
      if (node is AnonymousObjectCreationExpressionSyntax anonymousObject)
      {
        return anonymousObject.Initializers.Count == 0;
      }

      return false;
    }
  }
}
