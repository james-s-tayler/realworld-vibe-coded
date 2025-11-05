using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that ensures SaveChanges/SaveChangesAsync is only called in UnitOfWork implementations.
  /// This centralizes transaction management and domain event dispatch.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class SaveChangesOnlyInUnitOfWorkAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "PV012";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "No SaveChanges/SaveChangesAsync outside UnitOfWork",
        "Call SaveChanges/SaveChangesAsync only in IUnitOfWork implementations. Use IUnitOfWork.ExecuteInTransactionAsync() to centralize transactions and domain event dispatch.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "SaveChanges/SaveChangesAsync on DbContext or IRepository should only be called within classes implementing IUnitOfWork to centralize transaction management and domain event dispatch.");

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

      // Get the method being called
      var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return;
      }

      // Check if it's SaveChanges or SaveChangesAsync
      if (methodSymbol.Name != "SaveChanges" && methodSymbol.Name != "SaveChangesAsync")
      {
        return;
      }

      // Check if it's from DbContext or IRepository
      var containingType = methodSymbol.ContainingType;
      if (containingType == null)
      {
        return;
      }

      // Check if the containing type is or derives from DbContext OR implements IRepository
      var isDbContext = IsDbContextOrDerived(containingType);
      var isRepository = IsRepositoryOrDerived(containingType);

      if (!isDbContext && !isRepository)
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

      // Check if we're inside a class that implements IUnitOfWork
      var enclosingClass = GetEnclosingClass(invocation);
      if (enclosingClass == null)
      {
        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
        return;
      }

      var classSymbol = context.SemanticModel.GetDeclaredSymbol(enclosingClass) as INamedTypeSymbol;
      if (classSymbol == null)
      {
        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
        return;
      }

      // Allow if the class itself is a DbContext (overriding the method)
      if (IsDbContextOrDerived(classSymbol))
      {
        return;
      }

      // Allow if the class itself is a Repository implementation (defining the method)
      if (IsRepositoryOrDerived(classSymbol))
      {
        return;
      }

      // Otherwise, must implement IUnitOfWork
      if (!ImplementsIUnitOfWork(classSymbol))
      {
        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsDbContextOrDerived(INamedTypeSymbol type)
    {
      var current = type;
      while (current != null)
      {
        if (current.Name == "DbContext" &&
            current.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore")
        {
          return true;
        }
        current = current.BaseType;
      }
      return false;
    }

    private static bool IsRepositoryOrDerived(INamedTypeSymbol type)
    {
      // Check if the type itself implements IRepository or IRepositoryBase interfaces
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

    private static bool ImplementsIUnitOfWork(INamedTypeSymbol type)
    {
      // Check if the class implements IUnitOfWork interface
      foreach (var @interface in type.AllInterfaces)
      {
        if (@interface.Name == "IUnitOfWork")
        {
          return true;
        }
      }

      // Also allow if the class name is UnitOfWork or ends with UnitOfWork
      if (type.Name == "UnitOfWork" || type.Name.EndsWith("UnitOfWork"))
      {
        return true;
      }

      return false;
    }

    private static ClassDeclarationSyntax GetEnclosingClass(SyntaxNode node)
    {
      var current = node.Parent;
      while (current != null)
      {
        if (current is ClassDeclarationSyntax classDecl)
        {
          return classDecl;
        }
        current = current.Parent;
      }
      return null;
    }
  }
}
