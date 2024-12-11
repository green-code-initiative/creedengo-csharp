namespace EcoCode.Analyzers;

/// <summary>GCI83 fixer: Replace enum ToString with nameof.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceEnumToStringWithNameOfFixer)), Shared]
public sealed class ReplaceEnumToStringWithNameOfFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [ReplaceEnumToStringWithNameOf.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0 || context.Document.GetLanguageVersion() < LanguageVersion.CSharp6)
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var nodeToFix = root.FindNode(context.Span, getInnermostNodeForTie: true);
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use nameof",
                createChangedDocument: token => RefactorAsync(context.Document, nodeToFix, token),
                equivalenceKey: "Use nameof"),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode node, CancellationToken token)
    {
        var editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);

        var newNode = editor.SemanticModel.GetOperation(node, token) switch
        {
            IInvocationOperation { Instance: { } } invocation => editor.Generator.NameOfExpression(invocation.Instance.Syntax),
            IInterpolationOperation interpolation => SyntaxFactory.Interpolation((ExpressionSyntax)editor.Generator.NameOfExpression(interpolation.Expression.Syntax)),
            _ => null,
        };

        if (newNode is null) return document;

        editor.ReplaceNode(node, newNode);
        return editor.GetChangedDocument();
    }
}
