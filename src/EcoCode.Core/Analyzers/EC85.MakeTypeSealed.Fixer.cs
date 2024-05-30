﻿using System.Diagnostics.CodeAnalysis;

namespace EcoCode.Analyzers;

/// <summary>EC85 fixer: Make type sealed.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTypeSealedFixer)), Shared]
public sealed class MakeTypeSealedFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [MakeTypeSealed.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage] 
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length != 0)
        {
            context.RegisterCodeFix(CodeAction.Create(
                title: "Make type sealed",
                createChangedSolution: token => SealDeclaration(context, token),
                equivalenceKey: "Make type sealed"),
                context.Diagnostics);
        }
        return Task.CompletedTask;
    }

    private static async Task<Solution> SealDeclaration(CodeFixContext context, CancellationToken token)
    {
        var solutionEditor = new SolutionEditor(context.Document.Project.Solution);
        var location = context.Diagnostics[0].Location;

        if (solutionEditor.OriginalSolution.GetDocument(location.SourceTree) is not Document document ||
            await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not SyntaxNode root ||
            root.FindNode(location.SourceSpan) is not TypeDeclarationSyntax declaration)
        {
            return solutionEditor.OriginalSolution;
        }

        var documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, token).ConfigureAwait(false);
        var newModifiers = documentEditor.Generator.GetModifiers(declaration).WithIsSealed(true);
        documentEditor.ReplaceNode(declaration, documentEditor.Generator.WithModifiers(declaration, newModifiers));

        return solutionEditor.GetChangedSolution();
    }
}
