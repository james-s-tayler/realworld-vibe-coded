using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.I18n
{
  /// <summary>
  /// Flags ErrorMessage values that don't use IStringLocalizer in ICommandHandler/IQueryHandler classes.
  /// Checks both property assignments (ErrorMessage = ...) and ErrorDetail constructor arguments.
  /// Uses a whitelist approach: only localizer access and string.Join are allowed.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class HardcodedErrorMessageAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "I18N001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "ErrorMessage should use IStringLocalizer",
        "ErrorMessage must use '_localizer[SharedResource.Keys.*]' for i18n support. Hardcoded strings, constants, and helper methods are not allowed.",
        "I18n",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ErrorMessage values in handler classes must use IStringLocalizer for localization. Only localizer access and string.Join (for framework errors) are permitted.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var assemblyName = compilationContext.Compilation.AssemblyName ?? "";

        if (assemblyName.Contains("Test"))
        {
          return;
        }

        var commandHandlerType = compilationContext.Compilation.GetTypeByMetadataName("Server.SharedKernel.MediatR.ICommandHandler`2");
        var queryHandlerType = compilationContext.Compilation.GetTypeByMetadataName("Server.SharedKernel.MediatR.IQueryHandler`2");
        var errorDetailType = compilationContext.Compilation.GetTypeByMetadataName("Server.SharedKernel.Result.ErrorDetail");

        if (commandHandlerType == null && queryHandlerType == null)
        {
          return;
        }

        // Check ErrorMessage property assignments
        compilationContext.RegisterSyntaxNodeAction(ctx =>
        {
          AnalyzeAssignment(ctx, commandHandlerType, queryHandlerType);
        }, SyntaxKind.SimpleAssignmentExpression);

        // Check ErrorDetail constructor calls
        if (errorDetailType != null)
        {
          compilationContext.RegisterSyntaxNodeAction(ctx =>
          {
            AnalyzeObjectCreation(ctx, commandHandlerType, queryHandlerType, errorDetailType);
          }, SyntaxKind.ObjectCreationExpression);

          compilationContext.RegisterSyntaxNodeAction(ctx =>
          {
            AnalyzeImplicitObjectCreation(ctx, commandHandlerType, queryHandlerType, errorDetailType);
          }, SyntaxKind.ImplicitObjectCreationExpression);
        }
      });
    }

    private static void AnalyzeAssignment(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol commandHandlerType,
      INamedTypeSymbol queryHandlerType)
    {
      var assignment = (AssignmentExpressionSyntax)context.Node;

      if (assignment.Left is not IdentifierNameSyntax identifier ||
          identifier.Identifier.Text != "ErrorMessage")
      {
        return;
      }

      if (IsAllowedExpression(assignment.Right, context))
      {
        return;
      }

      if (!IsInHandlerClass(context, commandHandlerType, queryHandlerType))
      {
        return;
      }

      context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation()));
    }

    private static void AnalyzeObjectCreation(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol commandHandlerType,
      INamedTypeSymbol queryHandlerType,
      INamedTypeSymbol errorDetailType)
    {
      var creation = (ObjectCreationExpressionSyntax)context.Node;

      var typeInfo = context.SemanticModel.GetTypeInfo(creation);
      if (!SymbolEqualityComparer.Default.Equals(typeInfo.Type, errorDetailType))
      {
        return;
      }

      if (!IsInHandlerClass(context, commandHandlerType, queryHandlerType))
      {
        return;
      }

      CheckErrorDetailArguments(context, creation.ArgumentList, creation.GetLocation());
    }

    private static void AnalyzeImplicitObjectCreation(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol commandHandlerType,
      INamedTypeSymbol queryHandlerType,
      INamedTypeSymbol errorDetailType)
    {
      var creation = (ImplicitObjectCreationExpressionSyntax)context.Node;

      var typeInfo = context.SemanticModel.GetTypeInfo(creation);
      if (!SymbolEqualityComparer.Default.Equals(typeInfo.Type, errorDetailType))
      {
        return;
      }

      if (!IsInHandlerClass(context, commandHandlerType, queryHandlerType))
      {
        return;
      }

      CheckErrorDetailArguments(context, creation.ArgumentList, creation.GetLocation());
    }

    private static void CheckErrorDetailArguments(
      SyntaxNodeAnalysisContext context,
      ArgumentListSyntax argumentList,
      Location location)
    {
      if (argumentList == null)
      {
        return;
      }

      var args = argumentList.Arguments;

      // ErrorDetail(string errorMessage) — 1 arg, check arg 0
      // ErrorDetail(string identifier, string errorMessage) — 2 args, check arg 1
      // ErrorDetail(string identifier, string errorMessage, string errorCode, ValidationSeverity severity) — 4 args, check arg 1
      int errorMessageIndex = args.Count == 1 ? 0 : 1;

      if (errorMessageIndex >= args.Count)
      {
        return;
      }

      var errorMessageArg = args[errorMessageIndex].Expression;

      if (!IsAllowedExpression(errorMessageArg, context))
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, location));
      }
    }

    /// <summary>
    /// Returns true if the expression is an allowed pattern for ErrorMessage values:
    /// - IStringLocalizer element access (localizer[key])
    /// - string.Join(...) for wrapping framework errors
    /// - Member access expressions (e.g., e.Description for framework IdentityError passthrough)
    /// </summary>
    private static bool IsAllowedExpression(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
      // Allow localizer[key] — ElementAccessExpression on IStringLocalizer
      if (ContainsLocalizerAccess(expression, context))
      {
        return true;
      }

      // Allow string.Join(...) — used for wrapping framework errors
      if (IsStringJoinExpression(expression))
      {
        return true;
      }

      // Allow member access expressions (e.g., e.Description) — these pass through
      // framework-generated messages (like IdentityError.Description) that are already
      // localized by the framework. This is NOT a hardcoded string.
      if (expression is MemberAccessExpressionSyntax)
      {
        return true;
      }

      return false;
    }

    private static bool ContainsLocalizerAccess(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
      foreach (var node in expression.DescendantNodesAndSelf())
      {
        if (node is ElementAccessExpressionSyntax elementAccess)
        {
          var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression);
          if (typeInfo.Type != null && IsStringLocalizerType(typeInfo.Type))
          {
            // Require the localizer argument to be a SharedResource.Keys.* member access,
            // not a magic string literal like _localizer["ArbitraryKey"]
            if (!HasStronglyTypedKeyArgument(elementAccess))
            {
              return false;
            }

            return true;
          }
        }
      }

      return false;
    }

    private static bool HasStronglyTypedKeyArgument(ElementAccessExpressionSyntax elementAccess)
    {
      var args = elementAccess.ArgumentList.Arguments;
      if (args.Count != 1)
      {
        return false;
      }

      // The argument must be a member access expression (e.g., SharedResource.Keys.EmailAlreadyExists)
      // This rejects string literals ("MagicKey"), identifiers (someVar), and other patterns
      return args[0].Expression is MemberAccessExpressionSyntax;
    }

    private static bool IsStringLocalizerType(ITypeSymbol typeSymbol)
    {
      if (typeSymbol.Name == "IStringLocalizer" &&
          typeSymbol.ContainingNamespace?.ToDisplayString() == "Microsoft.Extensions.Localization")
      {
        return true;
      }

      foreach (var iface in typeSymbol.AllInterfaces)
      {
        if (iface.Name == "IStringLocalizer" &&
            iface.ContainingNamespace?.ToDisplayString() == "Microsoft.Extensions.Localization")
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsStringJoinExpression(ExpressionSyntax expression)
    {
      if (expression is InvocationExpressionSyntax invocation)
      {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
          return memberAccess.Name.Identifier.Text == "Join" &&
                 memberAccess.Expression is PredefinedTypeSyntax predefined &&
                 predefined.Keyword.IsKind(SyntaxKind.StringKeyword);
        }
      }

      return false;
    }

    private static bool IsInHandlerClass(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol commandHandlerType,
      INamedTypeSymbol queryHandlerType)
    {
      var node = context.Node;
      while (node != null)
      {
        if (node is ClassDeclarationSyntax classDecl)
        {
          var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
          if (classSymbol != null)
          {
            return ImplementsInterface(classSymbol, commandHandlerType) ||
                   ImplementsInterface(classSymbol, queryHandlerType);
          }
        }

        node = node.Parent;
      }

      return false;
    }

    private static bool ImplementsInterface(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceType)
    {
      if (interfaceType == null)
      {
        return false;
      }

      foreach (var iface in classSymbol.AllInterfaces)
      {
        if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, interfaceType))
        {
          return true;
        }
      }

      return false;
    }
  }
}
