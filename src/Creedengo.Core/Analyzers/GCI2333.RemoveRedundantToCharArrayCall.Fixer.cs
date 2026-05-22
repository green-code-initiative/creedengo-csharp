namespace Creedengo.Core.Analyzers;

/// <summary>GC2333: Remove redundant 'ToCharArray' call.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantToCharArrayCallFixer)), Shared]
public sealed class RemoveRedundantToCharArrayCallFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ImmutableArray.Create(RemoveRedundantToCharArrayCall.Descriptor.Id);

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
                title: "Remove redundant 'ToCharArray' call",
                createChangedDocument: token => RefactorAsync(context.Document, nodeToFix, token),
                equivalenceKey: "Remove redundant 'ToCharArray' call"),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode nodeToFix, CancellationToken token)
    {
        var editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);

        // nodeToFix is the IdentifierNameSyntax "ToCharArray"; climb up to the InvocationExpressionSyntax
        if (nodeToFix.Parent is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Parent is not InvocationExpressionSyntax invocationSyntax)
        {
            return document;
        }

        if (editor.SemanticModel.GetOperation(invocationSyntax, token) is not IInvocationOperation invocation ||
            invocation.Arguments.Length != 0 ||
            invocation.Instance is null ||
            invocation.TargetMethod.Name != "ToCharArray")
        {
            return document;
        }

        editor.ReplaceNode(invocationSyntax, invocation.Instance.Syntax);
        return editor.GetChangedDocument();
    }
}