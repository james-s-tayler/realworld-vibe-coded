using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.I18n
{
  /// <summary>
  /// Warns when ErrorMessage is assigned a string literal instead of a localized value from IStringLocalizer.
  /// Helps ensure all error messages are localizable.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class HardcodedErrorMessageAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "I18N001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "ErrorMessage should use IStringLocalizer",
        "ErrorMessage is assigned a hardcoded string. Use '_localizer[SharedResource.Keys.{0}]' instead for i18n support.",
        "I18n",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ErrorMessage properties should use IStringLocalizer for localization instead of hardcoded string literals.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var assemblyName = compilationContext.Compilation.AssemblyName ?? "";

        // Exclude test projects
        if (assemblyName.Contains("Test"))
        {
          return;
        }

        var commandHandlerType = compilationContext.Compilation.GetTypeByMetadataName("Server.SharedKernel.MediatR.ICommandHandler`2");
        var queryHandlerType = compilationContext.Compilation.GetTypeByMetadataName("Server.SharedKernel.MediatR.IQueryHandler`2");

        if (commandHandlerType == null && queryHandlerType == null)
        {
          return;
        }

        compilationContext.RegisterSyntaxNodeAction(ctx =>
        {
          AnalyzeAssignment(ctx, commandHandlerType, queryHandlerType);
        }, SyntaxKind.SimpleAssignmentExpression);
      });
    }

    private static void AnalyzeAssignment(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol commandHandlerType,
      INamedTypeSymbol queryHandlerType)
    {
      var assignment = (AssignmentExpressionSyntax)context.Node;

      // Check if left side is ErrorMessage
      if (assignment.Left is not IdentifierNameSyntax identifier ||
          identifier.Identifier.Text != "ErrorMessage")
      {
        return;
      }

      // Check if right side is a string literal or interpolated string
      if (assignment.Right is not LiteralExpressionSyntax &&
          assignment.Right is not InterpolatedStringExpressionSyntax)
      {
        return;
      }

      // Allow string.Join(...) — used for wrapping framework errors
      if (IsStringJoinExpression(assignment.Right))
      {
        return;
      }

      // Check if containing class implements ICommandHandler or IQueryHandler
      if (!IsInHandlerClass(context, commandHandlerType, queryHandlerType))
      {
        return;
      }

      var messageText = assignment.Right is LiteralExpressionSyntax literal
          ? literal.Token.ValueText
          : "...";

      var diagnostic = Diagnostic.Create(Rule, assignment.GetLocation(), messageText);
      context.ReportDiagnostic(diagnostic);
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
