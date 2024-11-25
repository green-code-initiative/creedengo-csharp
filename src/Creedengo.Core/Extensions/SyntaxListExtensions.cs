namespace Creedengo.Core.Extensions;

/// <summary>Extensions methods for <see cref="SyntaxList{TNode}"/>.</summary>
public static class SyntaxListExtensions
{
    /// <summary>Returns the single node of the list, default if empty or more than one node is contained.</summary>
    /// <typeparam name="TNode">The node type.</typeparam>
    /// <param name="list">The syntax list.</param>
    /// <returns>The single node of the list, default if empty or more than one node is contained.</returns>
    public static TNode? SingleOrDefaultNoThrow<TNode>(this SyntaxList<TNode> list)
        where TNode : SyntaxNode => list.Count == 1 ? list[0] : default;
}
