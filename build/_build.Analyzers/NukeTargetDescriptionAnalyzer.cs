using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NukeTargetDescriptionAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MissingDescriptionRule = new DiagnosticDescriptor(
        "NUKE001",
        "Nuke target is missing description",
        "Target '{0}' must include a .Description() call to provide user documentation",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All Nuke build targets should have a .Description() call to document their purpose for users.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(MissingDescriptionRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        
        // Check if this is a Target property (has type "Target")
        if (!IsTargetProperty(propertyDeclaration))
            return;

        // Get the property name
        var propertyName = propertyDeclaration.Identifier.ValueText;
        
        // Skip if this is an overridden property or has certain attributes that indicate it's not a user target
        if (ShouldSkipTarget(propertyName))
            return;

        // Check if the property expression contains a .Description() call
        if (!HasDescriptionCall(propertyDeclaration))
        {
            var diagnostic = Diagnostic.Create(
                MissingDescriptionRule, 
                propertyDeclaration.Identifier.GetLocation(), 
                propertyName);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsTargetProperty(PropertyDeclarationSyntax property)
    {
        // Check if the property type is "Target"
        return property.Type is IdentifierNameSyntax identifierName && 
               identifierName.Identifier.ValueText == "Target";
    }

    private static bool ShouldSkipTarget(string propertyName)
    {
        // Skip certain system/default targets that don't need descriptions
        var skipTargets = new[] { "Clean", "Restore", "Compile" };
        return skipTargets.Contains(propertyName);
    }

    private static bool HasDescriptionCall(PropertyDeclarationSyntax property)
    {
        // Look for .Description() calls in the property expression
        var expressionBody = property.ExpressionBody?.Expression;
        if (expressionBody != null)
        {
            return ContainsDescriptionCall(expressionBody);
        }

        // Check accessor with lambda expression body
        var getter = property.AccessorList?.Accessors
            .FirstOrDefault(a => a.Keyword.IsKind(SyntaxKind.GetKeyword));
        
        if (getter?.ExpressionBody?.Expression != null)
        {
            return ContainsDescriptionCall(getter.ExpressionBody.Expression);
        }

        return false;
    }

    private static bool ContainsDescriptionCall(SyntaxNode node)
    {
        // Recursively search for .Description() calls in the syntax tree
        if (node is InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.ValueText == "Description")
            {
                return true;
            }
        }

        // Search child nodes recursively
        return node.ChildNodes().Any(ContainsDescriptionCall);
    }
}