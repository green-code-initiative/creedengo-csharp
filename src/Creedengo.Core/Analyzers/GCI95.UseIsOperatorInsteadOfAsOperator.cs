namespace Creedengo.Core.Analyzers;

/// <summary>GCI95: Use is operator instead of as operator.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIsOperatorInsteadOfAsOperator : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI95_UseIsOperatorInsteadOfAsOperator,
        title: "Use 'is' operator instead of 'as' operator",
        message: "'As' is used instead of 'is' to check type.",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Use 'is' instead of 'as' to improve readability and performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    private static readonly ImmutableArray<SyntaxKind> SupportedSyntaxKinds = ImmutableArray.Create(
        SyntaxKind.IfStatement,
        SyntaxKind.ConditionalExpression,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement,
        SyntaxKind.ForStatement,
        SyntaxKind.ReturnStatement);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeNode(context), SupportedSyntaxKinds);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var condition = context.Node switch
        {
            IfStatementSyntax ifStatement => ifStatement.Condition,
            ForStatementSyntax forStatement => forStatement.Condition,
            WhileStatementSyntax whileStatement => whileStatement.Condition,
            DoStatementSyntax dowhileStatement => dowhileStatement.Condition,
            ConditionalExpressionSyntax conditionalExpression => conditionalExpression.Condition,
            ReturnStatementSyntax returnStatement => returnStatement.Expression,
            _ => null
        };

        if (condition == null) return;

        foreach (var node in condition.DescendantNodesAndSelf())
        {
            if (node is not BinaryExpressionSyntax binaryExpr) continue;

            var left = binaryExpr.Left;
            var right = binaryExpr.Right;

            if (IsAsExpressionComparedToNull(left, right) || IsAsExpressionComparedToNull(right, left))
            {
                var asExpr = (left is BinaryExpressionSyntax lAs && lAs.IsKind(SyntaxKind.AsExpression)) ? lAs
                    : (right is BinaryExpressionSyntax rAs && rAs.IsKind(SyntaxKind.AsExpression)) ? rAs
                    : null;

                if (asExpr is not null)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, asExpr.GetLocation()));
            }
        }
    }

    private static bool IsAsExpressionComparedToNull(ExpressionSyntax expressionA, ExpressionSyntax expressionB) =>
        expressionA is BinaryExpressionSyntax binaryExpressionSyntax && binaryExpressionSyntax.IsKind(SyntaxKind.AsExpression) &&
        expressionB is LiteralExpressionSyntax literalExpressionSyntax &&
        literalExpressionSyntax.IsKind(SyntaxKind.NullLiteralExpression);
}
