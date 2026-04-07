using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RequireTestCoverageAttributeAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "E2E008";

  public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      "[TestCoverage] attribute is required on all [Fact] test methods",
      "Test method '{0}' is missing [TestCoverage] attribute. Add [TestCoverage(Id = \"...\", FeatureArea = \"...\", Behavior = \"...\")] to document what this test verifies.",
      "Documentation",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Every [Fact] test method must have a [TestCoverage] attribute describing what behavior it verifies. This enables programmatic extraction of the test coverage master list.");

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
      ImmutableArray.Create(Rule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
  }

  private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
  {
    var method = (MethodDeclarationSyntax)context.Node;

    var hasFact = false;
    var hasTestCoverage = false;

    foreach (var attributeList in method.AttributeLists)
    {
      foreach (var attribute in attributeList.Attributes)
      {
        var name = attribute.Name.ToString();
        if (name is "Fact" or "FactAttribute")
        {
          hasFact = true;
        }
        else if (name is "TestCoverage" or "TestCoverageAttribute")
        {
          hasTestCoverage = true;
        }
      }
    }

    if (hasFact && !hasTestCoverage)
    {
      var diagnostic = Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text);
      context.ReportDiagnostic(diagnostic);
    }
  }
}
