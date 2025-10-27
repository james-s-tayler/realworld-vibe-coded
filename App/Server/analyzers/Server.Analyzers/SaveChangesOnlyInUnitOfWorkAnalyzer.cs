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
        "Call SaveChanges/SaveChangesAsync only in IUnitOfWork implementations. Use IUnitOfWork.CommitAsync() to centralize transactions and domain event dispatch.",
        "Persistence",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "DbContext.SaveChanges/SaveChangesAsync should only be called within classes implementing IUnitOfWork to centralize transaction management and domain event dispatch.");

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

      // Check if it's from DbContext
      var containingType = methodSymbol.ContainingType;
      if (containingType == null)
      {
        return;
      }

      // Check if the containing type is or derives from DbContext
      if (!IsDbContextOrDerived(containingType))
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
