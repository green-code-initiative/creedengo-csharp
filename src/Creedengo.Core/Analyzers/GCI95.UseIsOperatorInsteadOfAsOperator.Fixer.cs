namespace Creedengo.Core.Analyzers;

/// <summary>
/// GCI95 fixer: Use Length to test empty strings.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseIsOperatorInsteadOfAsOperatorFixer)), Shared]
public sealed class UseIsOperatorInsteadOfAsOperatorFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [UseIsOperatorInsteadOfAsOperator.Descriptor.Id];

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0)
        {
            return;
        }

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                         .ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the expression with 'as'
        var node = root.FindNode(diagnosticSpan);
        if (node is not BinaryExpressionSyntax asExpression || asExpression.Kind() != SyntaxKind.AsExpression)
        {
            return;
        }

        // Find the parent binary expression: "x as T != null"
        var parentBinary = asExpression.Parent as BinaryExpressionSyntax;
        if (parentBinary is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use 'is' instead of 'as'",
                createChangedDocument: c => ReplaceWithIsOperatorAsync(context.Document, parentBinary, c),
                equivalenceKey: "UseIsOperatorInsteadOfAs"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithIsOperatorAsync(Document document, BinaryExpressionSyntax binaryExpression, CancellationToken cancellationToken)
    {
        var left = binaryExpression.Left;
        var right = binaryExpression.Right;

        // Determine the 'as' part and the type being cast
        ExpressionSyntax? asExpr = null;
        ExpressionSyntax? comparedTo = null;

        if (left is BinaryExpressionSyntax lAs && lAs.IsKind(SyntaxKind.AsExpression))
        {
            asExpr = lAs;
            comparedTo = right;
        }
        else if (right is BinaryExpressionSyntax rAs && rAs.IsKind(SyntaxKind.AsExpression))
        {
            asExpr = rAs;
            comparedTo = left;
        }

        if (asExpr is not BinaryExpressionSyntax asExpressionSyntax || comparedTo is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return document;
        }

        // We assume form: (x as SomeType) != null
        var expression = asExpressionSyntax.Left;
        var type = asExpressionSyntax.Right;

        var isExpression = SyntaxFactory.BinaryExpression(
            SyntaxKind.IsExpression,
            expression.WithoutTrivia(),
            type.WithoutTrivia()
        ).WithTriviaFrom(binaryExpression);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root.ReplaceNode(binaryExpression, isExpression);

        return document.WithSyntaxRoot(newRoot);
    }
}
