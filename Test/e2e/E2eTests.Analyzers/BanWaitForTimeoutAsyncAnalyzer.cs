using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BanWaitForTimeoutAsyncAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "E2E001";

  public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      "WaitForTimeoutAsync is not allowed",
      "Do not use Page.WaitForTimeoutAsync(). Use proper Playwright waiting strategies like Expect(...).ToBeVisibleAsync() or WaitForURLAsync() instead.",
      "Reliability",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "WaitForTimeoutAsync is a code smell that leads to flaky tests. Tests should wait for specific conditions using Playwright assertions instead of arbitrary time delays.");

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

    // Check if the method name is WaitForTimeoutAsync
    if (memberAccess.Name.Identifier.Text != "WaitForTimeoutAsync")
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
