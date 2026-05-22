using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Creedengo.Core.Analyzers;

/// <summary>GCI6605 fixer: Replace Any with Exists on List&lt;T&gt;.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExistsInsteadOfAnyFixer)), Shared]
public sealed class UseExistsInsteadOfAnyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ImmutableArray.Create(UseExistsInsteadOfAny.Descriptor.Id);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.FirstOrDefault() is not { } diagnostic ||
            await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root ||
            root.FindNode(context.Span, getInnermostNodeForTie: true) is not SimpleNameSyntax { Identifier.Text: "Any" } anyName)
        {
            return;
        }

        context.RegisterCodeFix(CodeAction.Create(
            "Use Exists instead of Any",
            ct => ReplaceAnyWithExistsAsync(context.Document, anyName),
            equivalenceKey: "Use Exists instead of Any"),
            diagnostic);
    }

    private static async Task<Document> ReplaceAnyWithExistsAsync(Document document, SimpleNameSyntax anyName)
    {
        var existsIdentifier = SyntaxFactory.IdentifierName("Exists").WithTriviaFrom(anyName);
        return await document.WithUpdatedRoot(anyName, existsIdentifier).ConfigureAwait(false);
    }
}
