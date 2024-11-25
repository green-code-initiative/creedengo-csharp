using System.Runtime.InteropServices;

namespace Creedengo.Core.Analyzers;

/// <summary>EC81 fixer: Use struct layout.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SpecifyStructLayoutFixer)), Shared]
public sealed class SpecifyStructLayoutFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [SpecifyStructLayout.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var node = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (node is not TypeDeclarationSyntax nodeToFix) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add Auto StructLayout attribute",
                token => RefactorAsync(context.Document, nodeToFix, LayoutKind.Auto, token),
                equivalenceKey: "Add Auto StructLayout attribute"),
            context.Diagnostics);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add Sequential StructLayout attribute",
                token => RefactorAsync(context.Document, nodeToFix, LayoutKind.Sequential, token),
                equivalenceKey: "Add Sequential StructLayout attribute"),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode nodeToFix, LayoutKind layoutKind, CancellationToken token)
    {
        var editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);

        var structLayoutAttributeType = editor.SemanticModel.Compilation.GetBestTypeByMetadataName("System.Runtime.InteropServices.StructLayoutAttribute");
        if (structLayoutAttributeType is null) return document;

        var layoutKindType = editor.SemanticModel.Compilation.GetBestTypeByMetadataName("System.Runtime.InteropServices.LayoutKind");
        if (layoutKindType is null) return document;

        editor.AddAttribute(nodeToFix, editor.Generator.Attribute(
            editor.Generator.TypeExpression(structLayoutAttributeType).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation),
            [
                editor.Generator.AttributeArgument(editor.Generator.MemberAccessExpression(
                    editor.Generator.TypeExpression(layoutKindType).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation),
                    layoutKind.ToString())),
            ]));
        return editor.GetChangedDocument();
    }
}
