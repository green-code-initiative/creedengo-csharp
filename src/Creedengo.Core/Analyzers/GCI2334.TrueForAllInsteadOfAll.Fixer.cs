namespace Creedengo.Core.Analyzers;

/// <summary>GCI2334: Use 'TrueForAll' instead of 'All' on a List.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TrueForAllInsteadOfAllFixer)), Shared]
public sealed class TrueForAllInsteadOfAllFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ImmutableArray.Create(TrueForAllInsteadOfAll.Descriptor.Id);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0)
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var nodeToFix = root.FindNode(context.Span, getInnermostNodeForTie: true);
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use 'TrueForAll' instead of 'All'",
                createChangedDocument: token => RefactorAsync(context.Document, nodeToFix, token),
                equivalenceKey: "Use 'TrueForAll' instead of 'All'"),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode nodeToFix, CancellationToken token)
    {
        // nodeToFix is the IdentifierNameSyntax "All"; climb up to the InvocationExpressionSyntax
        if (nodeToFix.Parent is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Parent is not InvocationExpressionSyntax invocation)
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);

        var newName = SyntaxFactory.IdentifierName("TrueForAll").WithTriviaFrom(memberAccess.Name);
        editor.ReplaceNode(invocation, invocation.WithExpression(memberAccess.WithName(newName)));

        return editor.GetChangedDocument();
    }
}
