using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that detects blocking on async Entity Framework calls using .Result or .Wait().
  /// This can cause deadlocks and thread pool starvation.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BlockOnAsyncEfCallsAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV023";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Don't block on async EF calls",
        "Avoid using .Result or .Wait() on Entity Framework async methods. Use await instead to prevent deadlocks and thread pool starvation.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Blocking on async Entity Framework operations using .Result or .Wait() can cause deadlocks and thread pool starvation. Always use await with CancellationToken.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
      var memberAccess = (MemberAccessExpressionSyntax)context.Node;

      // Check if accessing .Result or .Wait()
      var memberName = memberAccess.Name.Identifier.Text;
      if (memberName != "Result" && memberName != "Wait")
      {
        return;
      }

      // Get the type of the expression being accessed
      var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
      if (typeInfo.Type == null)
      {
        return;
      }

      // Check if it's a Task type
      var type = typeInfo.Type;
      if (type.Name != "Task" || type.ContainingNamespace?.ToDisplayString() != "System.Threading.Tasks")
      {
        return;
      }

      // Check if the original invocation is an EF method
      var expression = memberAccess.Expression;

      // Walk up to find the invocation
      while (expression is MemberAccessExpressionSyntax innerMemberAccess)
      {
        expression = innerMemberAccess.Expression;
      }

      if (expression is InvocationExpressionSyntax invocation)
      {
        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol != null && IsEfAsyncMethod(methodSymbol))
        {
          var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
      // Also check the expression itself if it's an invocation
      else if (memberAccess.Expression is InvocationExpressionSyntax directInvocation)
      {
        var methodSymbol = context.SemanticModel.GetSymbolInfo(directInvocation).Symbol as IMethodSymbol;
        if (methodSymbol != null && IsEfAsyncMethod(methodSymbol))
        {
          var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static bool IsEfAsyncMethod(IMethodSymbol method)
    {
      // Check for common EF async methods
      var efAsyncMethods = new[]
      {
        "ToListAsync",
        "ToArrayAsync",
        "FirstAsync",
        "FirstOrDefaultAsync",
        "SingleAsync",
        "SingleOrDefaultAsync",
        "AnyAsync",
        "AllAsync",
        "CountAsync",
        "LongCountAsync",
        "SumAsync",
        "AverageAsync",
        "MinAsync",
        "MaxAsync",
        "FindAsync",
        "AddAsync",
        "AddRangeAsync",
        "SaveChangesAsync",
        "ForEachAsync",
        "LoadAsync"
      };

      if (System.Array.IndexOf(efAsyncMethods, method.Name) < 0)
      {
        return false;
      }

      // Check if it's from EF Core or System.Linq namespace
      var containingNamespace = method.ContainingType?.ContainingNamespace?.ToDisplayString();
      return containingNamespace != null &&
             (containingNamespace.StartsWith("Microsoft.EntityFrameworkCore") ||
              containingNamespace == "System.Linq");
    }
  }
}
