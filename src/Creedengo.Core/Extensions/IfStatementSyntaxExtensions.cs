namespace Creedengo.Core.Extensions;
internal static class IfStatementSyntaxExtensions
{
    /// <summary>
    /// Returns true if the specified if statement is a simple if statement.
    /// Simple if statement is defined as follows: it is not a child of an else clause and it has no else clause.
    /// </summary>
    public static bool IsSimpleIf(this IfStatementSyntax ifStatement) => ifStatement?.IsParentKind(SyntaxKind.ElseClause) == false
            && ifStatement.Else is null;
}
