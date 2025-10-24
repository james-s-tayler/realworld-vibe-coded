using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that enforces all MediatR requests to implement IResultRequest.
  /// This ensures consistent Result-based return types across all MediatR handlers.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class MediatRResultRequestAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV004";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "MediatR request must implement IResultRequest",
        "The type '{0}' used with MediatR.Send must be assignable to IResultRequest. All MediatR requests should return Result<T> to ensure consistent error handling and response patterns.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All MediatR requests must implement IResultRequest to ensure they return Result<T>. This provides consistent error handling, validation, and response patterns across the application.");

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

      // Check if this is a call to Send method
      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      // Check if the method name is "Send"
      if (memberAccess.Name.Identifier.Text != "Send")
      {
        return;
      }

      // Get the symbol information
      var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
      var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

      if (methodSymbol == null)
      {
        return;
      }

      // Check if this is IMediator.Send or ISender.Send (MediatR)
      var containingTypeName = methodSymbol.ContainingType?.Name;
      var containingNamespace = methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString();

      if (containingNamespace != "MediatR" ||
          (containingTypeName != "IMediator" && containingTypeName != "ISender"))
      {
        return;
      }

      // Get the first argument (the request)
      if (invocation.ArgumentList.Arguments.Count == 0)
      {
        return;
      }

      var firstArgument = invocation.ArgumentList.Arguments[0].Expression;
      var typeInfo = context.SemanticModel.GetTypeInfo(firstArgument);
      var requestType = typeInfo.Type;

      if (requestType == null)
      {
        return;
      }

      // Check if the request type implements IResultRequest
      if (!ImplementsIResultRequest(requestType))
      {
        var diagnostic = Diagnostic.Create(
            Rule,
            firstArgument.GetLocation(),
            requestType.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool ImplementsIResultRequest(ITypeSymbol typeSymbol)
    {
      // Check the type itself
      if (IsIResultRequest(typeSymbol))
      {
        return true;
      }

      // Check all interfaces
      foreach (var interfaceType in typeSymbol.AllInterfaces)
      {
        if (IsIResultRequest(interfaceType))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsIResultRequest(ITypeSymbol typeSymbol)
    {
      // Check if it's IResultRequest (non-generic or generic)
      if (typeSymbol.Name == "IResultRequest" &&
          typeSymbol.ContainingNamespace?.ToDisplayString() == "Server.SharedKernel")
      {
        return true;
      }

      return false;
    }
  }
}
