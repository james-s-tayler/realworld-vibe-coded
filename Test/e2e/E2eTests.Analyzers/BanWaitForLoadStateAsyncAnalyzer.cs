using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BanWaitForLoadStateAsyncAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "E2E003";

  public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      "WaitForLoadStateAsync is not allowed",
      "Do not use Page.WaitForLoadStateAsync(). Use Playwright's Expect() assertions like Expect(locator).ToBeVisibleAsync() instead.",
      "Reliability",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "WaitForLoadStateAsync leads to flaky tests. Use Playwright's Expect() assertions to wait for specific elements or conditions instead.");

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

    // Check if this is a method call
    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
    {
      return;
    }

    // Check if the method name is WaitForLoadStateAsync
    if (memberAccess.Name.Identifier.Text != "WaitForLoadStateAsync")
    {
      return;
    }

    // Get the method symbol to verify it's from Playwright's IPage
    var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
    if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
    {
      return;
    }

    // Check if the method belongs to IPage or Page from Playwright
    var containingType = methodSymbol.ContainingType;
    if (containingType == null)
    {
      return;
    }

    // Check if it's from Microsoft.Playwright namespace
    var namespaceName = containingType.ContainingNamespace?.ToDisplayString();
    if (namespaceName == null || !namespaceName.StartsWith("Microsoft.Playwright"))
    {
      return;
    }

    var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
    context.ReportDiagnostic(diagnostic);
  }
}
