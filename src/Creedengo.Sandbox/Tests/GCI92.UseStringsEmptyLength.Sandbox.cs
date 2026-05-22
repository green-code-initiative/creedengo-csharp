namespace Creedengo.Sandbox.Tests;

// Mirrors GCI92.UseStringsEmptyLength.Tests.cs.
// Comparisons marked "warns" should light up GCI92 in the IDE; "ok" comparisons should stay clean.
internal static class GCI92Sandbox
{
    public static void Positive_EqualsEmptyString()
    {
        string test = "test";
        if (test == "") { } // warns
    }

    public static void Positive_EqualsEmptyStringReversed()
    {
        string test = "test";
        if ("" == test) { } // warns
    }

    public static void Positive_NotEqualsEmptyString()
    {
        string test = "test";
        if (test != "") { } // warns
        if ("" != test) { } // warns
    }

    public static void Negative_LengthCheck()
    {
        string test = "test";
        if (test.Length == 0) { } // ok — already using Length
        if (test.Length != 0) { } // ok
    }
}
