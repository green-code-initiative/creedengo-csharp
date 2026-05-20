namespace Creedengo.Core.Analyzers;

/// <summary>GCI2508 fixer: Remove useless ToString call.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveUselessToStringCallFixer)), Shared]
public sealed class RemoveUselessToStringCallFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ImmutableArray.Create(RemoveUselessToStringCall.Descriptor.Id);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var nodeToFix = root.FindNode(context.Span, getInnermostNodeForTie: true);
        if (nodeToFix is not InvocationExpressionSyntax syntax)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove useless ToString call",
                createChangedDocument: token => RefactorAsync(context.Document, syntax, token),
                equivalenceKey: RemoveUselessToStringCall.Descriptor.Id),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, InvocationExpressionSyntax syntax, CancellationToken token)
    {
        var editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);

        if (syntax.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var receiver = memberAccess.Expression;

            // If the invocation is the whole expression statement (e.g. `s.ToString();`), remove the statement entirely.
            if (syntax.Parent is ExpressionStatementSyntax exprStmt)
            {
                editor.RemoveNode(exprStmt, SyntaxRemoveOptions.KeepExteriorTrivia);
                return editor.GetChangedDocument();
            }

            // Fallback: replace invocation with receiver, preserving trivia.
            editor.ReplaceNode(syntax, receiver.WithTriviaFrom(syntax));
        }

        return editor.GetChangedDocument();
    }
}
