using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Testing
{
  /// <summary>
  /// Analyzer that bans direct instantiation of DateTime using the new keyword in test code.
  /// Tests should use DateTime.Parse with human-readable date strings instead.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanNewDateTimeAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV011";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Direct DateTime instantiation is not allowed in test code",
        "Do not use 'new DateTime()'. Use DateTime.Parse with human-readable date strings instead (e.g., 'DateTime.Parse(\"2023-01-01\")') for more maintainable tests.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Direct instantiation of DateTime objects using the 'new' keyword should be avoided in test code. Using DateTime.Parse with human-readable date strings makes tests more readable and maintainable.");

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

        // Register for object creation to catch new DateTime()
        compilationContext.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
      });
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
      var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

      var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
      var typeSymbol = typeInfo.Type;

      // Check if creating new DateTime
      if (typeSymbol?.Name == "DateTime" &&
          typeSymbol?.ContainingNamespace?.ToDisplayString() == "System")
      {
        var diagnostic = Diagnostic.Create(Rule, objectCreation.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
