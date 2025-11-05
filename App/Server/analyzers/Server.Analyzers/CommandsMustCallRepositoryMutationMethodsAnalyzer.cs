using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that ensures ICommand handlers call at least one repository mutation method
  /// (AddAsync, AddRangeAsync, UpdateAsync, UpdateRangeAsync, DeleteAsync, DeleteRangeAsync).
  /// This ensures that commands actually persist changes to the database.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class CommandsMustCallRepositoryMutationMethodsAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV014";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Commands must call repository mutation methods",
        "ICommand<{0}> handler '{1}' must call at least one repository mutation method (AddAsync, AddRangeAsync, UpdateAsync, UpdateRangeAsync, DeleteAsync, DeleteRangeAsync) to persist changes",
        "Architecture",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Command handlers should call at least one repository mutation method to ensure changes are tracked by EF Core. Without these calls, the UnitOfWork's SaveChangesAsync won't persist any changes.");

    private static readonly string[] MutationMethods = new[]
    {
      "AddAsync",
      "AddRangeAsync",
      "UpdateAsync",
      "UpdateRangeAsync",
      "DeleteAsync",
      "DeleteRangeAsync"
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
      var methodDeclaration = (MethodDeclarationSyntax)context.Node;

      // Only analyze methods named "Handle"
      if (methodDeclaration.Identifier.Text != "Handle")
      {
        return;
      }

      // Get the method symbol
      var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
      if (methodSymbol == null)
      {
        return;
      }

      // Check if the containing class implements ICommandHandler<TCommand, TResult>
      var containingType = methodSymbol.ContainingType;
      if (!ImplementsICommandHandler(containingType, out var resultType))
      {
        return;
      }

      // Allow in test projects
      var compilation = context.Compilation;
      var assemblyName = compilation.AssemblyName ?? string.Empty;
      if (assemblyName.Contains("Test") || assemblyName.Contains("Tests"))
      {
        return;
      }

      // Check if the method body contains calls to repository mutation methods
      if (methodDeclaration.Body == null && methodDeclaration.ExpressionBody == null)
      {
        return;
      }

      var hasMutationCall = false;

      // Get all invocation expressions in the method
      var invocations = methodDeclaration.DescendantNodes()
          .OfType<InvocationExpressionSyntax>();

      foreach (var invocation in invocations)
      {
        var invokedMethod = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (invokedMethod == null)
        {
          continue;
        }

        // Check if it's one of the mutation methods
        if (MutationMethods.Contains(invokedMethod.Name))
        {
          // Check if it's called on IRepository
          var receiverType = invokedMethod.ContainingType;
          if (IsRepositoryType(receiverType))
          {
            hasMutationCall = true;
            break;
          }
        }
      }

      if (!hasMutationCall)
      {
        var location = methodDeclaration.Identifier.GetLocation();
        var diagnostic = Diagnostic.Create(
            Rule,
            location,
            resultType?.Name ?? "T",
            containingType.Name);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool ImplementsICommandHandler(INamedTypeSymbol type, out ITypeSymbol resultType)
    {
      resultType = null;

      foreach (var @interface in type.AllInterfaces)
      {
        // Check for ICommandHandler<TCommand, TResult>
        if (@interface.Name == "ICommandHandler" && @interface.TypeArguments.Length == 2)
        {
          // The first type argument should implement ICommand<TResult>
          var commandType = @interface.TypeArguments[0] as INamedTypeSymbol;
          if (commandType != null)
          {
            foreach (var commandInterface in commandType.AllInterfaces)
            {
              if (commandInterface.Name == "ICommand" && commandInterface.TypeArguments.Length == 1)
              {
                resultType = commandInterface.TypeArguments[0];
                return true;
              }
            }
          }
        }
      }

      return false;
    }

    private static bool IsRepositoryType(INamedTypeSymbol type)
    {
      // Check if the type implements IRepository or IRepositoryBase
      foreach (var @interface in type.AllInterfaces)
      {
        var interfaceName = @interface.Name;
        var namespaceName = @interface.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Check for IRepository from Server.SharedKernel.Persistence
        if (interfaceName == "IRepository" && namespaceName.Contains("Server.SharedKernel"))
        {
          return true;
        }

        // Check for IRepositoryBase from Ardalis.Specification
        if (interfaceName == "IRepositoryBase" && namespaceName.Contains("Ardalis.Specification"))
        {
          return true;
        }
      }

      // Check if the type is or derives from RepositoryBase (Ardalis.Specification)
      var current = type;
      while (current != null)
      {
        var typeName = current.Name;
        var namespaceName = current.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Check for RepositoryBase from Ardalis.Specification
        if (typeName.Contains("RepositoryBase") && namespaceName.Contains("Ardalis.Specification"))
        {
          return true;
        }

        current = current.BaseType;
      }

      return false;
    }
  }
}
