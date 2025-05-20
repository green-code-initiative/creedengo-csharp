
using Microsoft.CodeAnalysis.CSharp;

namespace Creedengo.Tests.Tests;

/// <summary>GCI92: Use Length to test empty strings.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIsOperatorInsteadOfAsOperator : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI94_UseIsOperatorInsteadOfAsOperator,
        title: "Use 'is' operator instead of 'as' operator",
        message: "'As' is used instead of 'is' to check type.",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Use 'is' instead of 'as' to improve readability and performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        SyntaxKind[] supportSyntaxKinds = [
            SyntaxKind.IfStatement,
            SyntaxKind.ConditionalExpression,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.ForStatement
        ];

        foreach (var item in supportSyntaxKinds)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, item);
        }
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var condition = context.Node switch
        {
            IfStatementSyntax ifStatement => ifStatement.Condition,
            ForStatementSyntax forStatement => forStatement.Condition,
            WhileStatementSyntax whileStatement => whileStatement.Condition,
            DoStatementSyntax dowhileStatement => dowhileStatement.Condition,
            ConditionalExpressionSyntax conditionalExpression => conditionalExpression.Condition,
            _ => null
        };

        if (condition == null)
        {
            return;
        }

         foreach (var node in GetAllNodes(condition))
        {
            if (node is BinaryExpressionSyntax binaryExpr)
            {
                ExpressionSyntax left = binaryExpr.Left;
                ExpressionSyntax right = binaryExpr.Right;

                if (IsAsExpressionComparedToNull(left, right) || IsAsExpressionComparedToNull(right, left))
                {
                    // Diagnostic sur la partie "as"
                    BinaryExpressionSyntax? asExpr;
                    if (left is BinaryExpressionSyntax lAs && lAs.Kind() == SyntaxKind.AsExpression)
                    {
                        asExpr = lAs;
                    }
                    else
                    {
                        asExpr = right is BinaryExpressionSyntax rAs && rAs.Kind() == SyntaxKind.AsExpression ? rAs : null;
                    }

                    if (asExpr != null)
                    {
                        var diagnostic = Diagnostic.Create(Descriptor, asExpr.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
    private static IEnumerable<SyntaxNode> GetAllNodes(SyntaxNode root)
    {
        foreach (var descendant in root.DescendantNodesAndSelf())
        {
            yield return descendant;
        }
    }
    private static bool IsAsExpressionComparedToNull(ExpressionSyntax expressionA, ExpressionSyntax expressionB)
    {
        return expressionA is BinaryExpressionSyntax binaryExpressionSyntax && binaryExpressionSyntax.IsKind(SyntaxKind.AsExpression) &&
               expressionB is LiteralExpressionSyntax literalExpressionSyntax &&
               literalExpressionSyntax.IsKind(SyntaxKind.NullLiteralExpression);
    }
}
