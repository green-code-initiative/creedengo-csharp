namespace Creedengo.Extensions;

/// <summary>Extension methods for <see cref="Document"/>.</summary>
public static class DocumentExtensions
{
    /// <summary>Returns the language version of the document.</summary>
    /// <param name="document">The document.</param>
    /// <returns>The language version.</returns>
    public static LanguageVersion GetLanguageVersion(this Document document) =>
        document.Project.ParseOptions is CSharpParseOptions options ? options.LanguageVersion : LanguageVersion.Latest;

    /// <summary>Creates a new instance of this document with a root updated from the specified syntax nodes.</summary>
    /// <param name="document">The document.</param>
    /// <param name="oldNode">The old syntax node to replace.</param>
    /// <param name="newNode">The new syntax node to use.</param>
    public static async Task<Document> WithUpdatedRoot(this Document document, SyntaxNode oldNode, SyntaxNode newNode) =>
        await document.GetSyntaxRootAsync().ConfigureAwait(false) is { } root
        ? document.WithSyntaxRoot(root.ReplaceNode(oldNode, newNode))
        : document;
}
