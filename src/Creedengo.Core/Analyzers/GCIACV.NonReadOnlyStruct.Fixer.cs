namespace Creedengo.Core.Analyzers;

/// <summary>GCIACV fixer: Do not pass non-read-only struct by read-only reference.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NonReadOnlyStructFixer)), Shared]
public sealed class NonReadOnlyStructFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [NonReadOnlyStruct.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;
        
        var diagnostic = context.Diagnostics.First();
        var nodeSpan = diagnostic.Location.SourceSpan;
        var parameter = root.FindToken(nodeSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ParameterSyntax>()
            .FirstOrDefault();
            
        if (parameter == null) return;

        // Check if it's an 'in' parameter or 'ref readonly' parameter
        if (parameter.Modifiers.Any(SyntaxKind.InKeyword))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove 'in' modifier",
                    c => RemoveInModifierAsync(context.Document, parameter, c),
                    equivalenceKey: "Remove 'in' modifier"),
                diagnostic);
        }
        else if (parameter.Modifiers.Any(SyntaxKind.RefKeyword) && parameter.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove 'readonly' modifier (keep 'ref')",
                    c => RemoveReadOnlyModifierAsync(context.Document, parameter, c),
                    equivalenceKey: "Remove 'readonly' modifier"),
                diagnostic);
                
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove 'ref readonly' modifiers",
                    c => RemoveRefReadOnlyModifiersAsync(context.Document, parameter, c),
                    equivalenceKey: "Remove 'ref readonly' modifiers"),
                diagnostic);
        }
    }

    private static async Task<Document> RemoveInModifierAsync(
        Document document, 
        ParameterSyntax parameter, 
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        
        // Get the 'in' modifier
        var inModifier = parameter.Modifiers.First(m => m.IsKind(SyntaxKind.InKeyword));
        
        // Create new parameter without the 'in' modifier
        var newParameter = parameter.WithModifiers(parameter.Modifiers.Remove(inModifier));
        
        // Replace the parameter
        editor.ReplaceNode(parameter, newParameter);
        
        return editor.GetChangedDocument();
    }
    
    private static async Task<Document> RemoveReadOnlyModifierAsync(
        Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        
        // Get the 'readonly' modifier
        var readOnlyModifier = parameter.Modifiers.First(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
        
        // Create new parameter without the 'readonly' modifier
        var newParameter = parameter.WithModifiers(parameter.Modifiers.Remove(readOnlyModifier));
        
        // Replace the parameter
        editor.ReplaceNode(parameter, newParameter);
        
        return editor.GetChangedDocument();
    }
    
    private static async Task<Document> RemoveRefReadOnlyModifiersAsync(
        Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        
        // Get the 'ref' and 'readonly' modifiers
        var refModifier = parameter.Modifiers.First(m => m.IsKind(SyntaxKind.RefKeyword));
        var readOnlyModifier = parameter.Modifiers.First(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
        
        // Create new parameter without both modifiers
        var newParameter = parameter.WithModifiers(
            parameter.Modifiers.Remove(refModifier).Remove(readOnlyModifier));
        
        // Replace the parameter
        editor.ReplaceNode(parameter, newParameter);
        
        return editor.GetChangedDocument();
    }
}
