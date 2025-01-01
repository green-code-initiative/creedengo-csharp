namespace EcoCode.Analyzers;

/// <summary>GCI92 fixer: Use Length to test empty strings.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseLengthToTestEmptyStringsFixer)), Shared]
public sealed class UseLengthToTestEmptyStringsFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseLengthToTestEmptyStrings.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan) is not BinaryExpressionSyntax binaryExpr)
                continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use string.Length instead of comparison with empty string",
                    createChangedDocument: _ => ReplaceWithLengthCheckAsync(context.Document, binaryExpr),
                    equivalenceKey: "Use string.Length instead of comparison with empty string"),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceWithLengthCheckAsync(Document document, BinaryExpressionSyntax binaryExpr)
    {
        var (left, right) = (binaryExpr.Left, binaryExpr.Right);
        var stringExpr = left.IsEmptyStringLiteral() ? right : right.IsEmptyStringLiteral() ? left : null;
        return stringExpr is null ? document : await document.WithUpdatedRoot(binaryExpr, UpdateBinaryExpression(binaryExpr, stringExpr)).ConfigureAwait(false);

        static BinaryExpressionSyntax UpdateBinaryExpression(BinaryExpressionSyntax binaryExpr, ExpressionSyntax stringExpr) =>
            SyntaxFactory.BinaryExpression(binaryExpr.Kind(),
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringExpr, SyntaxFactory.IdentifierName("Length")),
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))
                .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
    }
}
