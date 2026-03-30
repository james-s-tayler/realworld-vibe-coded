using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.I18n
{
  /// <summary>
  /// Ensures that .WithMessage() calls in FastEndpoints Validator classes use a lambda
  /// when accessing IStringLocalizer, to avoid frozen culture values in singletons.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ValidatorLocalizerLambdaAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "I18N002";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "WithMessage must use lambda for IStringLocalizer in Validator",
        "WithMessage uses eager evaluation of IStringLocalizer. Use a lambda '.WithMessage(x => localizer[...])' to avoid frozen culture in singleton Validator.",
        "I18n",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "FastEndpoints Validators are singletons. IStringLocalizer values evaluated in the constructor are frozen to the culture at startup. Use a lambda to defer evaluation to request time.");

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

      // Check if this is a .WithMessage() call
      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
          memberAccess.Name.Identifier.Text != "WithMessage")
      {
        return;
      }

      // Must have exactly one argument
      var args = invocation.ArgumentList.Arguments;
      if (args.Count != 1)
      {
        return;
      }

      var argument = args[0].Expression;

      // If the argument is a lambda, it's correct — skip
      if (argument is LambdaExpressionSyntax)
      {
        return;
      }

      // Check if the argument contains an element access on IStringLocalizer (localizer["Key"])
      if (!ContainsLocalizerAccess(argument, context))
      {
        return;
      }

      // Check if we're in a Validator<T> class
      if (!IsInValidatorClass(context))
      {
        return;
      }

      var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
      context.ReportDiagnostic(diagnostic);
    }

    private static bool ContainsLocalizerAccess(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
      // Look for ElementAccessExpressionSyntax (localizer["Key"]) in the expression
      foreach (var node in expression.DescendantNodesAndSelf())
      {
        if (node is ElementAccessExpressionSyntax elementAccess)
        {
          var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression);
          if (typeInfo.Type != null && IsStringLocalizerType(typeInfo.Type))
          {
            return true;
          }
        }
      }

      return false;
    }

    private static bool IsStringLocalizerType(ITypeSymbol typeSymbol)
    {
      // Check the type itself and all its interfaces
      if (IsStringLocalizerInterface(typeSymbol))
      {
        return true;
      }

      foreach (var iface in typeSymbol.AllInterfaces)
      {
        if (IsStringLocalizerInterface(iface))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsStringLocalizerInterface(ITypeSymbol typeSymbol)
    {
      return typeSymbol.Name == "IStringLocalizer" &&
             typeSymbol.ContainingNamespace?.ToDisplayString() == "Microsoft.Extensions.Localization";
    }

    private static bool IsInValidatorClass(SyntaxNodeAnalysisContext context)
    {
      var node = context.Node;
      while (node != null)
      {
        if (node is ClassDeclarationSyntax classDecl)
        {
          var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
          if (classSymbol != null)
          {
            return InheritsFromValidator(classSymbol);
          }
        }

        node = node.Parent;
      }

      return false;
    }

    private static bool InheritsFromValidator(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        // FastEndpoints Validator<T>
        if (baseType.Name == "Validator" &&
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
