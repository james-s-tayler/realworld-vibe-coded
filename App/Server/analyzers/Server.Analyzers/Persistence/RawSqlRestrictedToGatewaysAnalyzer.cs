using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that restricts raw SQL API usage to designated gateway classes.
  /// This centralizes and audits FromSqlRaw/Interpolated and ExecuteSql* usage.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RawSqlRestrictedToGatewaysAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV003";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Raw SQL APIs restricted to gateway classes",
        "Raw SQL method '{0}' should only be used in classes marked with [SqlGateway] or in approved gateway namespaces. Move to a gateway class or wrap in an approved helper to centralize and audit SQL usage.",
        "Persistence",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Raw SQL APIs like FromSqlRaw, FromSqlInterpolated, ExecuteSqlRaw, ExecuteSqlRawAsync should be centralized in gateway classes marked with [SqlGateway] attribute or within configured namespaces for auditing and security.");

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

      // Get the method being called
      var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return;
      }

      // Check if it's a raw SQL method
      var rawSqlMethods = new[]
      {
        "FromSqlRaw",
        "FromSqlRawAsync",
        "FromSqlInterpolated",
        "FromSqlInterpolatedAsync",
        "ExecuteSqlRaw",
        "ExecuteSqlRawAsync",
        "ExecuteSqlInterpolated",
        "ExecuteSqlInterpolatedAsync"
      };

      if (System.Array.IndexOf(rawSqlMethods, methodSymbol.Name) < 0)
      {
        return;
      }

      // Check if it's from EF Core
      var containingNamespace = methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty;
      if (!containingNamespace.StartsWith("Microsoft.EntityFrameworkCore"))
      {
        return;
      }

      // Check if we're in an approved class
      var enclosingClass = GetEnclosingClass(invocation);
      if (enclosingClass == null)
      {
        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);
        context.ReportDiagnostic(diagnostic);
        return;
      }

      var classSymbol = context.SemanticModel.GetDeclaredSymbol(enclosingClass) as INamedTypeSymbol;
      if (classSymbol != null && !IsApprovedForRawSql(classSymbol))
      {
        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsApprovedForRawSql(INamedTypeSymbol type)
    {
      // Check for [SqlGateway] attribute
      foreach (var attribute in type.GetAttributes())
      {
        var attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (attributeName == "SqlGateway" || attributeName == "SqlGatewayAttribute")
        {
          return true;
        }
      }

      // Check if class name ends with "Gateway" or "SqlGateway"
      if (type.Name.EndsWith("Gateway") || type.Name.EndsWith("SqlGateway"))
      {
        return true;
      }

      // Check if in approved namespace (e.g., .Data.Queries or .Infrastructure.Data.Queries)
      var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
      if (namespaceName.Contains(".Data.Queries") ||
          namespaceName.Contains(".Infrastructure.Data.Queries"))
      {
        return true;
      }

      return false;
    }

    private static ClassDeclarationSyntax GetEnclosingClass(SyntaxNode node)
    {
      var current = node.Parent;
      while (current != null)
      {
        if (current is ClassDeclarationSyntax classDecl)
        {
          return classDecl;
        }
        current = current.Parent;
      }
      return null;
    }
  }
}
