namespace Creedengo.Core.Analyzers;

/// <summary>EC88 fixer: Dispose resource asynchronously.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposeResourceAsynchronouslyFixer)), Shared]
public sealed class DisposeResourceAsynchronouslyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [DisposeResourceAsynchronously.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent is not { } parent)
                continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is UsingStatementSyntax usingStatement)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Dispose resource asynchronously",
                            createChangedDocument: _ => context.Document.WithUpdatedRoot(usingStatement, usingStatement
                                .WithoutTrivia() // Needs to be removed then re-added to keep everything ordered
                                .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
                                .WithTriviaFrom(usingStatement)),
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
                if (node is LocalDeclarationStatementSyntax usingDeclaration)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Dispose resource asynchronously",
                            createChangedDocument: _ => context.Document.WithUpdatedRoot(usingDeclaration, usingDeclaration
                                .WithoutTrivia() // Needs to be removed then re-added to keep everything ordered
                                .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
                                .WithTriviaFrom(usingDeclaration)),
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
            }
        }
    }
}
