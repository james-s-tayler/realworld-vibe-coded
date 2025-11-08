using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Testing
{
  /// <summary>
  /// Analyzer that bans raw usage of HttpClient in functional tests.
  /// HttpClient should only be used as properties or fields in test fixtures,
  /// and test code should use FastEndpoints extension methods (POSTAsync, GETAsync, etc.)
  /// instead of raw HttpClient methods.
  /// This ensures consistent test patterns and encourages using the FastEndpoints testing utilities.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanRawHttpClientAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV007";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Raw HttpClient usage is not allowed in functional tests",
        "Do not use raw HttpClient methods. Use FastEndpoints extension methods like POSTAsync, GETAsync, PUTAsync, DELETEAsync, or PATCHAsync instead for better test readability and consistency.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Raw usage of HttpClient methods (SendAsync, GetAsync, PostAsync, etc.) should be avoided in functional tests. Instead, use FastEndpoints testing extension methods which provide better type safety, automatic serialization, and more readable test code. HttpClient is allowed as properties or fields in test fixtures.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      // Register for invocation expressions to catch HttpClient method calls
      context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);

      // Register for object creation to catch new HttpClient()
      context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);

      // Register for local variable declarations (HttpClient variable = ...)
      context.RegisterSyntaxNodeAction(AnalyzeLocalVariable, SyntaxKind.LocalDeclarationStatement);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
      var invocation = (InvocationExpressionSyntax)context.Node;

      // Check if this is a member access expression
      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      // Get the symbol for the method being called
      var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return;
      }

      // Check if the method is from HttpClient class
      if (methodSymbol.ContainingType?.Name == "HttpClient" &&
          methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() == "System.Net.Http")
      {
        // List of banned HttpClient methods
        var bannedMethods = new[]
        {
          "SendAsync",
          "GetAsync",
          "PostAsync",
          "PutAsync",
          "DeleteAsync",
          "PatchAsync",
          "GetStringAsync",
          "GetByteArrayAsync",
          "GetStreamAsync"
        };

        if (System.Array.IndexOf(bannedMethods, methodSymbol.Name) >= 0)
        {
          var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
      var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

      var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
      var typeSymbol = typeInfo.Type;

      // Check if creating new HttpClient()
      if (typeSymbol?.Name == "HttpClient" &&
          typeSymbol?.ContainingNamespace?.ToDisplayString() == "System.Net.Http")
      {
        var diagnostic = Diagnostic.Create(Rule, objectCreation.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static void AnalyzeLocalVariable(SyntaxNodeAnalysisContext context)
    {
      var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

      // Check if we're in a property or field (which are allowed)
      // Walk up to see if this is inside a property or field initializer
      var parent = localDeclaration.Parent;
      while (parent != null)
      {
        if (parent is PropertyDeclarationSyntax || parent is FieldDeclarationSyntax)
        {
          // This is inside a property or field, which is allowed
          return;
        }
        parent = parent.Parent;
      }

      // Get the type of the variable
      var variableType = localDeclaration.Declaration.Type;
      var typeInfo = context.SemanticModel.GetTypeInfo(variableType);
      var typeSymbol = typeInfo.Type;

      // Check if it's HttpClient type
      if (typeSymbol?.Name == "HttpClient" &&
          typeSymbol?.ContainingNamespace?.ToDisplayString() == "System.Net.Http")
      {
        // Allow var declarations if they're assigned from a method that returns HttpClient
        // (like CreateClient() from test fixtures)
        if (variableType.IsVar)
        {
          // Check if the initializer is a method call to CreateClient or similar
          var variable = localDeclaration.Declaration.Variables.FirstOrDefault();
          if (variable?.Initializer?.Value is InvocationExpressionSyntax invocation)
          {
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol?.Name == "CreateClient" ||
                methodSymbol?.ContainingType?.Name == "WebApplicationFactory" ||
                methodSymbol?.ContainingType?.BaseType?.Name == "WebApplicationFactory")
            {
              // This is CreateClient from WebApplicationFactory, which is allowed
              return;
            }
          }
        }

        var diagnostic = Diagnostic.Create(Rule, variableType.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
