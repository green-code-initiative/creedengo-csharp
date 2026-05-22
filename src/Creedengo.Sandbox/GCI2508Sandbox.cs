namespace Creedengo.Sandbox;

// Mirrors GCI2508.RemoveUselessToStringCall.Tests.cs.
// Lines marked "warns" should light up GCI2508 in the IDE; "ok" lines should stay clean.
internal static class GCI2508Sandbox
{
    private const string SourceField = "a";
    private static readonly string Field = SourceField.ToString(); // warns — field initializer

    public static string ExpressionBodied
    {
        get
        {
            string s = "a";
            return s.ToString(); // warns — return statement
        }
    }

    public static void Positive_PassedAsArgument()
    {
        string s = "a";
        System.Console.Write(s.ToString()); // warns
    }

    public static void Positive_BareStatement()
    {
        string s = "a";
        s.ToString(); // warns — whole statement, fixer removes it
    }

    public static void Positive_Discard()
    {
        string s = "a";
        _ = s.ToString(); // warns
    }

    public static void Positive_LocalVariable()
    {
        string s = "a";
        string s2 = s.ToString(); // warns
        System.Console.Write(s2);
    }

    public static void Negative_OverloadWithCulture()
    {
        string s = "a";
        _ = s.ToString(System.Globalization.CultureInfo.CurrentCulture); // ok — parameterized overload
    }

    public static void Negative_NullConditional()
    {
        string? s = "a";
        _ = s?.ToString(); // ok — conditional access
    }

    public static void Negative_CastToObject()
    {
        string s = "a";
        ((object)s).ToString(); // ok — receiver is object, not string
    }

    public static void Negative_NonString()
    {
        int i = 3;
        _ = i.ToString(); // ok — receiver isn't string
    }
}
