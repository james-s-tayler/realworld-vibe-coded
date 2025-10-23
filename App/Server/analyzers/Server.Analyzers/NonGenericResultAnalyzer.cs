using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class NonGenericResultAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Non-generic Result type is not allowed",
        "Use 'Result<T>' instead of non-generic 'Result'. For void operations, use 'Result<Unit>'.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Non-generic Result is deprecated in Ardalis.Result v10+. Use Result<T> for type safety. For operations without a return value, use Result<Unit>.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      // Register for generic name syntax (e.g., ICommand<Result>, Task<Result>)
      context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);

      // Register for return types and parameter types
      context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
      context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext context)
    {
      var genericName = (GenericNameSyntax)context.Node;

      // Check if any type argument is the non-generic Result
      foreach (var typeArg in genericName.TypeArgumentList.Arguments)
      {
        if (IsNonGenericResult(typeArg, context.SemanticModel))
        {
          var diagnostic = Diagnostic.Create(Rule, typeArg.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
      var method = (MethodDeclarationSyntax)context.Node;

      // Check return type
      if (IsNonGenericResult(method.ReturnType, context.SemanticModel))
      {
        var diagnostic = Diagnostic.Create(Rule, method.ReturnType.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }

      // Check parameters
      foreach (var parameter in method.ParameterList.Parameters)
      {
        if (parameter.Type != null && IsNonGenericResult(parameter.Type, context.SemanticModel))
        {
          var diagnostic = Diagnostic.Create(Rule, parameter.Type.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
      var property = (PropertyDeclarationSyntax)context.Node;

      // Check property type
      if (IsNonGenericResult(property.Type, context.SemanticModel))
      {
        var diagnostic = Diagnostic.Create(Rule, property.Type.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsNonGenericResult(TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
      // Get type info
      var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
      var typeSymbol = typeInfo.Type;

      if (typeSymbol == null)
      {
        return false;
      }

      // Check if it's the non-generic Result type from Ardalis.Result
      // The non-generic Result is actually Result (not Result<T>)
      if (typeSymbol.Name == "Result" &&
          typeSymbol.ContainingNamespace?.ToDisplayString() == "Ardalis.Result")
      {
        // Check if it's NOT a generic type (i.e., it's the non-generic Result)
        if (!(typeSymbol is INamedTypeSymbol namedType) || !namedType.IsGenericType)
        {
          return true;
        }
      }

      return false;
    }
  }
}
