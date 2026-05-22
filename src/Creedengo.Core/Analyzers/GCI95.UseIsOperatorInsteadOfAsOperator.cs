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

    // The pattern we look for is `x as T == null` or `x as T != null` (and their reversed forms).
    // Subscribing directly to the two BinaryExpression kinds avoids the previous pattern of
    // intercepting six statement kinds and walking their condition expressions.
    private static readonly ImmutableArray<SyntaxKind> BinaryComparisons = ImmutableArray.Create(
        SyntaxKind.EqualsExpression,
        SyntaxKind.NotEqualsExpression);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeComparison, BinaryComparisons);
    }

    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        var binaryExpr = (BinaryExpressionSyntax)context.Node;
        var asExpr = AsExpressionComparedToNull(binaryExpr.Left, binaryExpr.Right)
                  ?? AsExpressionComparedToNull(binaryExpr.Right, binaryExpr.Left);
        if (asExpr is not null)
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, asExpr.GetLocation()));
    }

    private static BinaryExpressionSyntax? AsExpressionComparedToNull(ExpressionSyntax a, ExpressionSyntax b) =>
        a is BinaryExpressionSyntax asExpr && asExpr.IsKind(SyntaxKind.AsExpression) &&
        b is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression)
            ? asExpr
            : null;
}
