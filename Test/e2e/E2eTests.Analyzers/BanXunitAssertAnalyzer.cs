using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BanXunitAssertAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "E2E006";

  public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      "xUnit Assert is not allowed",
      "Do not use xUnit Assert methods. Use Playwright's Expect() assertions like Expect(locator).ToHaveTextAsync() instead.",
      "Reliability",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "xUnit Assert methods do not integrate with Playwright's auto-waiting mechanism. Use Playwright's Expect() assertions for reliable E2E tests.");

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

    // Check if this is a method call on Assert
    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
    {
      return;
    }

    // Get the method symbol
    var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
    if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
    {
      return;
    }

    // Check if the method belongs to xUnit's Assert class
    var containingType = methodSymbol.ContainingType;
    if (containingType == null)
    {
      return;
    }

    // Check if it's the xUnit Assert class
    if (containingType.Name != "Assert")
    {
      return;
    }

    // Check if it's from Xunit namespace
    var namespaceName = containingType.ContainingNamespace?.ToDisplayString();
    if (namespaceName != "Xunit")
    {
      return;
    }

    var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
    context.ReportDiagnostic(diagnostic);
  }
}
