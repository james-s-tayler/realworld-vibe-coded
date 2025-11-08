using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Testing
{
  /// <summary>
  /// Analyzer that bans usage of xunit.Assert in test code.
  /// Tests should use Shouldly assertion methods instead for more readable and expressive tests.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanXunitAssertAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV010";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "xunit.Assert usage is not allowed in test code",
        "Do not use xunit.Assert. Use Shouldly assertion methods instead for more readable and expressive tests (e.g., 'result.ShouldBe(expected)' instead of 'Assert.Equal(expected, result)').",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Direct usage of xunit.Assert methods should be avoided in test code. Shouldly provides more readable and expressive assertion methods that produce better error messages and improve test maintainability.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      // Only analyze test projects
      context.RegisterCompilationStartAction(compilationContext =>
      {
        var compilation = compilationContext.Compilation;
        var assemblyName = compilation.AssemblyName;

        // Only apply this analyzer to test projects
        if (assemblyName == null ||
            (!assemblyName.Contains("Test") && !assemblyName.Contains("Tests")))
        {
          return;
        }

        // Register for member access to catch Assert.* calls
        compilationContext.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
      });
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
      var memberAccess = (MemberAccessExpressionSyntax)context.Node;

      // Check if this is accessing something from Assert class
      var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
      var symbol = symbolInfo.Symbol;

      if (symbol == null)
      {
        return;
      }

      // Check if the containing type is Assert from xunit namespace
      var containingType = symbol.ContainingType;
      if (containingType?.Name == "Assert" &&
          containingType?.ContainingNamespace?.ToDisplayString() == "Xunit")
      {
        var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
