using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Analyzer that enforces restricted usage of Send methods in FastEndpoints endpoint classes.
  /// Only ResultValueAsync and ResultMapperAsync are allowed from endpoint code.
  /// This ensures consistent result handling patterns and prevents direct usage of
  /// low-level FastEndpoints response methods (like Send.OkAsync, Send.ErrorsAsync, etc.)
  /// which bypass the standardized Result pattern handling.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class SendMethodUsageAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV003";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Disallowed Send method usage in endpoint",
        "Only 'Send.ResultValueAsync' and 'Send.ResultMapperAsync' are allowed in endpoint code. Use these methods to maintain consistent Result pattern handling instead of directly calling '{0}'.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Endpoints should only use ResultValueAsync or ResultMapperAsync extension methods to send responses. This ensures all responses go through the standardized Result pattern handling, which provides consistent error handling, status code mapping, and validation error formatting. Direct usage of FastEndpoints Send methods (like OkAsync, ErrorsAsync, NotFoundAsync, etc.) bypasses this standardization and should be avoided.");

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

      // Check if this is a member access expression (e.g., Send.SomeMethod)
      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      // Check if the expression is accessing Send property
      if (memberAccess.Expression is not IdentifierNameSyntax identifier ||
          identifier.Identifier.Text != "Send")
      {
        return;
      }

      var methodName = memberAccess.Name.Identifier.Text;

      // Allow ResultValueAsync and ResultMapperAsync
      if (methodName == "ResultValueAsync" || methodName == "ResultMapperAsync")
      {
        return;
      }

      // Check if we're in an endpoint class (inherits from FastEndpoints.Endpoint<...>)
      if (!IsInEndpointClass(context))
      {
        return;
      }

      // Report diagnostic for any other Send method
      var diagnostic = Diagnostic.Create(
          Rule,
          invocation.GetLocation(),
          $"Send.{methodName}");
      context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInEndpointClass(SyntaxNodeAnalysisContext context)
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
            // Check if this class or any base class is from FastEndpoints
            return InheritsFromEndpoint(classSymbol);
          }
        }
        node = node.Parent;
      }

      return false;
    }

    private static bool InheritsFromEndpoint(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        // Check if the base type name starts with "Endpoint"
        // and is from the FastEndpoints namespace
        if (baseType.Name.StartsWith("Endpoint") &&
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
