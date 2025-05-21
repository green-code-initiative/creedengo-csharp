namespace Creedengo.Core.Analyzers;

/// <summary>GCIS3962 fixer: Replace static readonly by const when possible.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceStaticReadonlyByConstFixer)), Shared]
public sealed class ReplaceStaticReadonlyByConstFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [ReplaceStaticReadonlyByConst.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var node = token.Parent?.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().FirstOrDefault();
            if (node is null) continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace static readonly by const",
                    createChangedDocument: ct => RefactorAsync(context.Document, node, ct),
                    equivalenceKey: "Replace static readonly by const"),
                diagnostic);
        }
    }

    private static async Task<Document> RefactorAsync(Document document, FieldDeclarationSyntax fieldDecl, CancellationToken token)
    {
        // Remove static and readonly, add const
        var modifiers = fieldDecl.Modifiers;
        modifiers = new SyntaxTokenList(modifiers.Where(m => !m.IsKind(SyntaxKind.StaticKeyword) && !m.IsKind(SyntaxKind.ReadOnlyKeyword)));
        var constToken = SyntaxFactory.Token(fieldDecl.GetLeadingTrivia(), SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        modifiers = modifiers.Insert(0, constToken);

        // If the type is 'var', replace with the actual type
        var declaration = fieldDecl.Declaration;
        var typeSyntax = declaration.Type;
        if (typeSyntax.IsVar)
        {
            var semanticModel = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
            var type = semanticModel?.GetTypeInfo(typeSyntax, token).ConvertedType;
            if (type != null && type.Name != "var")
            {
                declaration = declaration.WithType(
                    SyntaxFactory.ParseTypeName(type.ToDisplayString())
                        .WithLeadingTrivia(typeSyntax.GetLeadingTrivia())
                        .WithTrailingTrivia(typeSyntax.GetTrailingTrivia())
                        .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Simplification.Simplifier.Annotation));
            }
        }

        var newField = fieldDecl
            .WithModifiers(modifiers)
            .WithDeclaration(declaration)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        if (root is null) return document;
        var newRoot = root.ReplaceNode(fieldDecl, newField);
        return document.WithSyntaxRoot(newRoot);
    }
}
