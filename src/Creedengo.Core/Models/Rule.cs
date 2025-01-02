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
        public const string GCI69_DontCallFunctionsInLoopConditions = "GCI69";
        public const string GCI72_DontExecuteSqlCommandsInLoops = "GCI72";
        public const string GCI75_DontConcatenateStringsInLoops = "GCI75";
        public const string GCI81_UseStructLayout = "GCI81";
        public const string GCI82_VariableCanBeMadeConstant = "GCI82";
        public const string GCI83_ReplaceEnumToStringWithNameOf = "GCI83";
        public const string GCI84_AvoidAsyncVoidMethods = "GCI84";
        public const string GCI85_MakeTypeSealed = "GCI85";
        public const string GCI86_GCCollectShouldNotBeCalled = "GCI86";
        public const string GCI87_UseCollectionIndexer = "GCI87";
        public const string GCI88_DisposeResourceAsynchronously = "GCI88";
        public const string GCI89_DoNotPassMutableStructAsRefReadonly = "GCI89";
        public const string GCI91_UseWhereBeforeOrderBy = "GCI91";
        public const string GCI92_UseStringEmptyLength = "GCI92";
        public const string GCI93_ReturnTaskDirectly = "GCI93";
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
            $"https://github.com/green-code-initiative/creedengo-rules-specifications/blob/main/src/main/rules/{id}/csharp/{id}.asciidoc");
}
