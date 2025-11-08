using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Analyzer that detects usage of GetAwaiter().GetResult() in FastEndpoints.ResponseMapper classes.
  /// This pattern can cause deadlocks and poor asynchronous behavior in ResponseMapper classes.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ResponseMapperGetAwaiterAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV009";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "GetAwaiter().GetResult() is not allowed in ResponseMapper classes",
        "Usage of GetAwaiter().GetResult() is not allowed in ResponseMapper classes. Override FromEntityAsync and await the call instead.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using GetAwaiter().GetResult() within FastEndpoints.ResponseMapper classes can cause deadlocks and poor asynchronous behavior. Instead, override FromEntityAsync and use await for asynchronous calls.");

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

      // Check if this is a call to GetResult()
      if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
          memberAccess.Name.Identifier.Text == "GetResult")
      {
        // Check if the expression before GetResult() is a call to GetAwaiter()
        if (memberAccess.Expression is InvocationExpressionSyntax innerInvocation &&
            innerInvocation.Expression is MemberAccessExpressionSyntax innerMemberAccess &&
            innerMemberAccess.Name.Identifier.Text == "GetAwaiter")
        {
          // Check if we're in a ResponseMapper class
          if (IsInResponseMapperClass(context))
          {
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
          }
        }
      }
    }

    private static bool IsInResponseMapperClass(SyntaxNodeAnalysisContext context)
    {
      // Walk up the syntax tree to find the containing class
      var node = context.Node;
      while (node != null)
      {
        if (node is ClassDeclarationSyntax classDecl)
        {
          // Get the semantic model to check base types
          var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
          if (classSymbol != null)
          {
            // Check if this class inherits from ResponseMapper
            return InheritsFromResponseMapper(classSymbol);
          }
        }
        node = node.Parent;
      }

      return false;
    }

    private static bool InheritsFromResponseMapper(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        // Check if the base type name is ResponseMapper
        // and is from the FastEndpoints namespace
        if (baseType.Name == "ResponseMapper" &&
            baseType.ContainingNamespace?.ToDisplayString() == "FastEndpoints")
        {
          return true;
        }

        baseType = baseType.BaseType;
      }

      return false;
    }
  }
}
