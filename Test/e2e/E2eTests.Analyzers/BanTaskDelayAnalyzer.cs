using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BanTaskDelayAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "E2E005";

  public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      "Task.Delay is not allowed",
      "Do not use Task.Delay(). Use Playwright's Expect() assertions like Expect(locator).ToBeVisibleAsync() instead.",
      "Reliability",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Task.Delay leads to flaky tests. Use Playwright's Expect() assertions to wait for specific elements or conditions instead of arbitrary time delays.");

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

    // Check if the method name is Delay
    if (memberAccess.Name.Identifier.Text != "Delay")
    {
      return;
    }

    // Get the method symbol to verify it's from System.Threading.Tasks.Task
    var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
    if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
    {
      return;
    }

    // Check if the method belongs to Task
    var containingType = methodSymbol.ContainingType;
    if (containingType == null)
    {
      return;
    }

    // Check if it's from System.Threading.Tasks namespace
    var namespaceName = containingType.ContainingNamespace?.ToDisplayString();
    if (namespaceName != "System.Threading.Tasks")
    {
      return;
    }

    // Check if the containing type is Task
    if (containingType.Name != "Task")
    {
      return;
    }

    var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
    context.ReportDiagnostic(diagnostic);
  }
}
