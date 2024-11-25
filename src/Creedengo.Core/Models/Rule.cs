namespace Creedengo.Core.Models;

internal static class Rule
{
    public static class Categories
    {
        public const string Design = "Design";
        public const string Usage = "Usage";
        public const string Performance = "Performance";
    }

    public static class Ids
    {
        public const string EC69_DontCallFunctionsInLoopConditions = "EC69";
        public const string EC72_DontExecuteSqlCommandsInLoops = "EC72";
        public const string EC75_DontConcatenateStringsInLoops = "EC75";
        public const string EC81_UseStructLayout = "EC81";
        public const string EC82_VariableCanBeMadeConstant = "EC82";
        public const string EC83_ReplaceEnumToStringWithNameOf = "EC83";
        public const string EC84_AvoidAsyncVoidMethods = "EC84";
        public const string EC85_MakeTypeSealed = "EC85";
        public const string EC86_GCCollectShouldNotBeCalled = "EC86";
        public const string EC87_UseCollectionIndexer = "EC87";
        public const string EC88_DisposeResourceAsynchronously = "EC88";
        public const string EC89_DoNotPassMutableStructAsRefReadonly = "EC89";
        public const string EC91_UseWhereBeforeOrderBy = "EC91";
        public const string EC92_UseStringEmptyLength = "EC92";
        public const string EC93_ReturnTaskDirectly = "EC93";
    }

    /// <summary>Creates a diagnostic descriptor.</summary>
    /// <param name="id">The rule id.</param>
    /// <param name="title">The rule title.</param>
    /// <param name="message">The rule message.</param>
    /// <param name="category">The rule category.</param>
    /// <param name="severity">The rule severity.</param>
    /// <param name="description">The rule description.</param>
    /// <returns>The diagnostic descriptor.</returns>
    public static DiagnosticDescriptor CreateDescriptor(string id, string title, string message, string category, DiagnosticSeverity severity, string description) =>
        new(id, title, message, category, severity, isEnabledByDefault: true, description, helpLinkUri:
            $"https://github.com/green-code-initiative/ecoCode/blob/main/ecocode-rules-specifications/src/main/rules/{id}/csharp/{id}.asciidoc");
}
