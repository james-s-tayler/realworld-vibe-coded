using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that bans string-based Include calls in Entity Framework queries.
  /// String-based navigation property names are brittle and not refactoring-safe.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanStringBasedIncludeAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV032";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Ban string-based Include",
        "Use strongly typed Include/ThenInclude instead of string-based Include(\"{0}\")",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "String-based Include calls are brittle and not refactoring-safe. Use strongly typed Include<T> with lambda expressions instead.");

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

      // Check if this is a member access expression
      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      // Get the method symbol
      var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return;
      }

      // Check if it's the Include method from EntityFrameworkQueryableExtensions
      if (methodSymbol.Name != "Include")
      {
        return;
      }

      // Check if it's from Microsoft.EntityFrameworkCore namespace
      var containingType = methodSymbol.ContainingType;
      if (containingType?.ContainingNamespace?.ToDisplayString() != "Microsoft.EntityFrameworkCore")
      {
        return;
      }

      // Check if the first argument is a string literal or string expression
      if (invocation.ArgumentList.Arguments.Count > 0)
      {
        var firstArgument = invocation.ArgumentList.Arguments[0];
        var typeInfo = context.SemanticModel.GetTypeInfo(firstArgument.Expression);

        // If the argument type is string, it's a string-based Include
        if (typeInfo.Type?.SpecialType == SpecialType.System_String)
        {
          var propertyName = firstArgument.Expression.ToString().Trim('"');
          var diagnostic = Diagnostic.Create(Rule, firstArgument.GetLocation(), propertyName);
          context.ReportDiagnostic(diagnostic);
        }
      }
    }
  }
}
