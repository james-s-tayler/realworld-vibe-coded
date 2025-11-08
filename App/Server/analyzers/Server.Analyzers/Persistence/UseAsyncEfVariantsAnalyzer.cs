using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that detects synchronous EF methods being used in async methods.
  /// Async methods should use async EF variants to avoid sync blocking on I/O.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class UseAsyncEfVariantsAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV022";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Use async EF variants in async flows",
        "Use {0}Async instead of {0} in async methods to avoid sync blocking on I/O operations",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Synchronous EF methods like ToList(), First(), Single(), Any(), Count() should use their async variants (ToListAsync, FirstAsync, etc.) inside async methods to avoid blocking threads on I/O operations.");

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

      // Check if we're inside an async method
      if (!IsInAsyncMethod(invocation))
      {
        return;
      }

      // Get the method being called
      var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return;
      }

      // Check if it's a sync EF method that has an async variant
      var syncMethods = new[]
      {
        "ToList", "ToArray", "First", "FirstOrDefault",
        "Single", "SingleOrDefault", "Any", "All",
        "Count", "LongCount", "Sum", "Average",
        "Min", "Max"
      };

      if (System.Array.IndexOf(syncMethods, methodSymbol.Name) < 0)
      {
        return;
      }

      // Check if it's being called on IQueryable (EF context)
      if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
      {
        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
        if (typeInfo.Type != null && IsQueryableType(typeInfo.Type))
        {
          var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static bool IsInAsyncMethod(SyntaxNode node)
    {
      var current = node.Parent;
      while (current != null)
      {
        if (current is MethodDeclarationSyntax method)
        {
          return method.Modifiers.Any(m => CSharpExtensions.IsKind((SyntaxToken)m, SyntaxKind.AsyncKeyword));
        }

        if (current is LocalFunctionStatementSyntax localFunction)
        {
          return localFunction.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        }

        // Also check for lambda expressions with async
        if (current is ParenthesizedLambdaExpressionSyntax lambda)
        {
          return lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);
        }

        if (current is SimpleLambdaExpressionSyntax simpleLambda)
        {
          return simpleLambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);
        }

        current = current.Parent;
      }
      return false;
    }

    private static bool IsQueryableType(ITypeSymbol type)
    {
      // Check if it's IQueryable or IQueryable<T>
      if (type.Name == "IQueryable")
      {
        return true;
      }

      // Check if it implements IQueryable
      if (type is INamedTypeSymbol namedType)
      {
        foreach (var @interface in namedType.AllInterfaces)
        {
          if (@interface.Name == "IQueryable")
          {
            return true;
          }
        }
      }

      return false;
    }
  }
}
