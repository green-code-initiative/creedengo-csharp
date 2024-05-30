﻿using System.Diagnostics.CodeAnalysis;

namespace EcoCode.Analyzers;

/// <summary>EC84 fixer: Avoid async void methods.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidAsyncVoidMethodsFixer)), Shared]
public sealed class AvoidAsyncVoidMethodsFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [AvoidAsyncVoidMethods.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage] 
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var parent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            if (parent is null) continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is not MethodDeclarationSyntax declaration) continue;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Convert to async Task",
                        createChangedDocument: token => RefactorAsync(document, declaration, token),
                        equivalenceKey: "Convert to async Task"),
                    diagnostic);
                break;
            }
        }
    }

    private static async Task<Document> RefactorAsync(Document document, MethodDeclarationSyntax methodDecl, CancellationToken token)
    {
        // Note : it may seem a good idea to add the System.Thread.Tasks using directive if it isn't present yet
        // However it isn't properly doable because :
        // - It could be added as a global using in a different file, but Roslyn doesn't give easy access to those
        // - The user could have enabled the ImplicitUsings option, which makes the using directives both global and invisible to the analyzer
        // So as a result, we simply don't handle it

        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        return root is null ? document : document.WithSyntaxRoot(
            root.ReplaceNode(methodDecl, methodDecl.WithReturnType(
                SyntaxFactory.IdentifierName("Task") // Change the return type of the method to Task
                .WithLeadingTrivia(methodDecl.ReturnType.GetLeadingTrivia())
                .WithTrailingTrivia(methodDecl.ReturnType.GetTrailingTrivia()))));
    }
}
