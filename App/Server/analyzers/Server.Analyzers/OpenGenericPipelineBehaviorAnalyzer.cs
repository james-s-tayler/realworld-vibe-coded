using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that prevents registration of MediatR pipeline behaviours using open generics.
  /// Open generic registrations like AddScoped(typeof(IPipelineBehavior&lt;,&gt;), typeof(MyBehavior&lt;,&gt;))
  /// can lead to runtime issues and ambiguity in behaviour resolution.
  /// Instead, use closed generic types or reflection-based registration with specific types.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class OpenGenericPipelineBehaviorAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV006";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Do not register MediatR pipeline behaviours using open generics",
        "Avoid registering IPipelineBehavior using open generic types like 'typeof(IPipelineBehavior<,>)'. This pattern can cause runtime issues and ambiguity in behaviour resolution. Use closed generic registrations or reflection-based registration with specific request/response types instead.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Open generic registrations of IPipelineBehavior (e.g., AddScoped(typeof(IPipelineBehavior<,>), typeof(MyBehavior<,>))) are not recommended in MediatR as they can cause subtle bugs and ambiguous behaviour resolution at runtime. Instead, register behaviours for specific request/response type pairs using closed generics, or use reflection at startup to register them for discovered types.");

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

      // Check if this is a member access expression (e.g., services.AddScoped(...))
      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      var methodName = memberAccess.Name.Identifier.Text;

      // Check if this is a DI registration method (AddScoped, AddTransient, AddSingleton)
      if (methodName != "AddScoped" && methodName != "AddTransient" && methodName != "AddSingleton")
      {
        return;
      }

      // Get the method symbol to verify it's from Microsoft.Extensions.DependencyInjection
      var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
      var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

      if (methodSymbol == null)
      {
        return;
      }

      // Check if this is from Microsoft.Extensions.DependencyInjection
      var containingNamespace = methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString();
      if (containingNamespace != "Microsoft.Extensions.DependencyInjection")
      {
        return;
      }

      // Check if the method has arguments
      if (invocation.ArgumentList.Arguments.Count < 1)
      {
        return;
      }

      // Check each argument for typeof(IPipelineBehavior<,>)
      foreach (var argument in invocation.ArgumentList.Arguments)
      {
        if (IsOpenGenericPipelineBehavior(argument.Expression, context.SemanticModel))
        {
          var diagnostic = Diagnostic.Create(
              Rule,
              argument.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static bool IsOpenGenericPipelineBehavior(ExpressionSyntax expression, SemanticModel semanticModel)
    {
      // Check if this is a typeof expression
      if (expression is not TypeOfExpressionSyntax typeofExpr)
      {
        return false;
      }

      var typeInfo = semanticModel.GetTypeInfo(typeofExpr.Type);
      var type = typeInfo.Type;

      if (type == null)
      {
        return false;
      }

      // Check if this is IPipelineBehavior
      if (!IsPipelineBehaviorType(type))
      {
        return false;
      }

      // Check if it's an open generic (has unbound type parameters)
      if (type is INamedTypeSymbol namedType)
      {
        // An open generic type will have TypeArguments that are type parameters
        if (namedType.IsGenericType && !namedType.IsUnboundGenericType)
        {
          // Check if any type argument is a type parameter (not bound to a specific type)
          foreach (var typeArg in namedType.TypeArguments)
          {
            if (typeArg.TypeKind == TypeKind.TypeParameter)
            {
              return true; // This is an open generic
            }
          }
        }
        // Check for unbound generic type definition (typeof(IPipelineBehavior<,>))
        else if (namedType.IsUnboundGenericType)
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsPipelineBehaviorType(ITypeSymbol typeSymbol)
    {
      // Check if the type itself is IPipelineBehavior
      if (IsIPipelineBehavior(typeSymbol))
      {
        return true;
      }

      // If it's a named type, also check if it's a generic type based on IPipelineBehavior
      if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
      {
        var originalDefinition = namedType.OriginalDefinition;
        if (IsIPipelineBehavior(originalDefinition))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsIPipelineBehavior(ITypeSymbol typeSymbol)
    {
      // Check if it's IPipelineBehavior from MediatR namespace
      if (typeSymbol.Name == "IPipelineBehavior" &&
          typeSymbol.ContainingNamespace?.ToDisplayString() == "MediatR")
      {
        return true;
      }

      return false;
    }
  }
}
