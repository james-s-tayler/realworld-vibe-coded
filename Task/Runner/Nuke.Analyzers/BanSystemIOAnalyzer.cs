using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BanSystemIOAnalyzer : DiagnosticAnalyzer
{
  public static readonly DiagnosticDescriptor SystemIORule = new DiagnosticDescriptor(
      "NUKE002",
      "Direct System.IO usage is not allowed",
      "Do not use '{0}' directly. Use Nuke.Common.IO.AbsolutePath extension methods instead.",
      "Usage",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "File system operations should use AbsolutePath extension methods from Nuke.Common.IO instead of direct System.IO classes for better cross-platform support and consistency.");

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
      ImmutableArray.Create(SystemIORule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
  }

  private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
  {
    var memberAccess = (MemberAccessExpressionSyntax)context.Node;

    // Check if this is a member access on System.IO types
    var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Expression);
    if (symbolInfo.Symbol == null)
      return;

    INamedTypeSymbol typeSymbol = null;
    if (symbolInfo.Symbol is INamedTypeSymbol type)
    {
      typeSymbol = type;
    }
    else if (symbolInfo.Symbol is IPropertySymbol property)
    {
      typeSymbol = property.Type as INamedTypeSymbol;
    }
    else if (symbolInfo.Symbol is IFieldSymbol field)
    {
      typeSymbol = field.Type as INamedTypeSymbol;
    }

    if (typeSymbol == null)
      return;

    // Check if the type is File, Directory, or Path from System.IO
    if (IsBannedSystemIOType(typeSymbol))
    {
      var diagnostic = Diagnostic.Create(
          SystemIORule,
          memberAccess.GetLocation(),
          typeSymbol.Name);

      context.ReportDiagnostic(diagnostic);
    }
  }

  private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
  {
    var invocation = (InvocationExpressionSyntax)context.Node;

    // Get the method being invoked
    var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
    if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
      return;

    // Check if the method belongs to a banned System.IO type
    if (IsBannedSystemIOType(methodSymbol.ContainingType))
    {
      var diagnostic = Diagnostic.Create(
          SystemIORule,
          invocation.GetLocation(),
          methodSymbol.ContainingType.Name);

      context.ReportDiagnostic(diagnostic);
    }
  }

  private static bool IsBannedSystemIOType(INamedTypeSymbol typeSymbol)
  {
    if (typeSymbol == null)
      return false;

    // Check if it's System.IO.File, System.IO.Directory, or System.IO.Path
    var bannedTypes = new[] { "File", "Directory", "Path" };
    return typeSymbol.ContainingNamespace?.ToString() == "System.IO" &&
           bannedTypes.Contains(typeSymbol.Name);
  }
}
