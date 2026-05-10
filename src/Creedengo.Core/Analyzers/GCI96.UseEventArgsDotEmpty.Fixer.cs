namespace Creedengo.Core.Analyzers;

/// <summary>
/// GCI96 fixer: Use 'EventArgs.Empty' instead of 'new EventArgs()'.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseEventArgsDotEmptyFixer)), Shared]
public sealed class UseEventArgsDotEmptyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(UseEventArgsDotEmpty.Descriptor.Id);

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        // The analyzer always reports on an ObjectCreationExpression node, so that's the only shape we need to handle.
        // getInnermostNodeForTie: true so that when the diagnostic span exactly matches an ArgumentSyntax (the wrapping
        // node when the creation expression is the entire argument), we descend into the inner ObjectCreationExpression
        // instead of returning the argument.
        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not ObjectCreationExpressionSyntax creation) continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Use 'EventArgs.Empty'",
                    c => ReplaceWithEventArgsEmptyAsync(context.Document, creation, c),
                    "UseEventArgsDotEmpty"),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceWithEventArgsEmptyAsync(
        Document document,
        SyntaxNode creation,
        CancellationToken cancellationToken)
    {
        var eventArgsEmpty = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("EventArgs"),
            SyntaxFactory.IdentifierName("Empty"));

        eventArgsEmpty = eventArgsEmpty.WithTriviaFrom(creation);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var newRoot = root.ReplaceNode(creation, eventArgsEmpty);
        return document.WithSyntaxRoot(newRoot);
    }

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}
