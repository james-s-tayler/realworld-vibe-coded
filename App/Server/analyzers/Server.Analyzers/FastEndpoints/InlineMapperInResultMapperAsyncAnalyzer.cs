using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class InlineMapperInResultMapperAsyncAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV018";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Inline mapper used with ResultMapperAsync",
        "Endpoint calls 'Send.ResultMapperAsync' with an inline mapper but is not defined as 'Endpoint<TRequest, TResponse, TMapper>'. Define the endpoint with a FastEndpoints.ResponseMapper<TResponse, TEntity> as the third type parameter.",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "FastEndpoints endpoints using ResultMapperAsync with inline lambda mappers should be refactored to use the three-parameter Endpoint<TRequest, TResponse, TMapper> pattern with a dedicated ResponseMapper class. This ensures consistent mapper architecture and enables mapper reusability.");

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

      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      if (memberAccess.Expression is not IdentifierNameSyntax identifier ||
          identifier.Identifier.Text != "Send")
      {
        return;
      }

      var methodName = memberAccess.Name.Identifier.Text;

      if (methodName != "ResultMapperAsync")
      {
        return;
      }

      if (!IsInEndpointClass(context, out var endpointSymbol))
      {
        return;
      }

      var arguments = invocation.ArgumentList.Arguments;
      if (arguments.Count < 2)
      {
        return;
      }

      var mapperArgument = arguments[1].Expression;

      if (!IsInlineMapper(mapperArgument))
      {
        return;
      }

      if (endpointSymbol != null && !UsesThreeParameterEndpointPattern(endpointSymbol))
      {
        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsInlineMapper(ExpressionSyntax expression)
    {
      if (expression is SimpleLambdaExpressionSyntax)
      {
        return true;
      }

      if (expression is ParenthesizedLambdaExpressionSyntax)
      {
        return true;
      }

      return false;
    }

    private static bool IsInEndpointClass(SyntaxNodeAnalysisContext context, out INamedTypeSymbol endpointSymbol)
    {
      endpointSymbol = null;

      var node = context.Node;
      while (node != null)
      {
        if (node is ClassDeclarationSyntax classDecl)
        {
          var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
          if (classSymbol != null && InheritsFromEndpoint(classSymbol))
          {
            endpointSymbol = classSymbol;
            return true;
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
        if (baseType.Name == "Endpoint" &&
            baseType.ContainingNamespace?.ToDisplayString() == "FastEndpoints")
        {
          return true;
        }

        baseType = baseType.BaseType;
      }

      return false;
    }

    private static bool UsesThreeParameterEndpointPattern(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        if (baseType.Name == "Endpoint" &&
            baseType.ContainingNamespace?.ToDisplayString() == "FastEndpoints")
        {
          return baseType.TypeArguments.Length == 3;
        }

        baseType = baseType.BaseType;
      }

      return false;
    }
  }
}
