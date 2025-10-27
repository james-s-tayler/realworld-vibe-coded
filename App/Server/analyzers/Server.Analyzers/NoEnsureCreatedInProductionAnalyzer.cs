using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that prevents usage of EnsureCreated/EnsureDeleted in production code.
  /// These methods bypass migrations and should only be used in tests.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class NoEnsureCreatedInProductionAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV042";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "No EnsureCreated/EnsureDeleted in production code",
        "Avoid using Database.{0} in production code. Use EF Core migrations instead, or guard behind environment checks for development-only scenarios.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Database.EnsureCreated and Database.EnsureDeleted bypass the migration system and can cause destructive operations. They should only be used in test projects or development-only code with proper environment guards.");

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

      // Check if it's EnsureCreated or EnsureDeleted
      if (methodSymbol.Name != "EnsureCreated" &&
          methodSymbol.Name != "EnsureCreatedAsync" &&
          methodSymbol.Name != "EnsureDeleted" &&
          methodSymbol.Name != "EnsureDeletedAsync")
      {
        return;
      }

      // Check if it's from DatabaseFacade (the Database property)
      var containingType = methodSymbol.ContainingType;
      if (containingType == null ||
          containingType.Name != "DatabaseFacade" ||
          !containingType.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore") == true)
      {
        return;
      }

      // Check if we're in a test project or DevOnly namespace
      var compilation = context.Compilation;
      var assemblyName = compilation.AssemblyName ?? string.Empty;

      // Allow in test projects
      if (assemblyName.Contains("Test") || assemblyName.Contains("Tests"))
      {
        return;
      }

      // Get the namespace of the containing class
      var enclosingClass = GetEnclosingClass(invocation);
      if (enclosingClass != null)
      {
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(enclosingClass) as INamedTypeSymbol;
        var namespaceName = classSymbol?.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Allow in DevOnly namespaces
        if (namespaceName.Contains("DevOnly"))
        {
          return;
        }
      }

      var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);
      context.ReportDiagnostic(diagnostic);
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
