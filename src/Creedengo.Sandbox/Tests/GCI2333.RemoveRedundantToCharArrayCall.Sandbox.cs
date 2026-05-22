namespace Creedengo.Sandbox.Tests;

// Mirrors GCI2333.RemoveRedundantToCharArrayCall.Tests.cs.
// Lines marked "warns" should light up GCI2333 in the IDE; "ok" lines should stay clean.
internal static class GCI2333Sandbox
{
    public static void Positive_OnVariable()
    {
        string s = "test";
        foreach (char c in s.ToCharArray()) // warns
            System.Console.WriteLine(c);
    }

    public static void Positive_OnStringLiteral()
    {
        foreach (char c in "hello".ToCharArray()) // warns
            System.Console.WriteLine(c);
    }

    public static void Positive_OnMethodReturn()
    {
        foreach (char c in GetText().ToCharArray()) // warns
            System.Console.WriteLine(c);
    }

    public static void Negative_PlainStringVariable()
    {
        string s = "test";
        foreach (char c in s) // ok — no ToCharArray
            System.Console.WriteLine(c);
    }

    public static void Negative_CharArrayVariable()
    {
        char[] chars = ['a', 'b'];
        foreach (char c in chars) // ok
            System.Console.WriteLine(c);
    }

    public static void Negative_BareMethodCall()
    {
        foreach (char c in GetChars()) // ok — not a member access
            System.Console.WriteLine(c);
    }

    public static void Negative_OtherStringMethod()
    {
        string s = "hello world";
        foreach (string part in s.Split(' ')) // ok — not ToCharArray
            System.Console.WriteLine(part);
    }

    public static void Negative_ToCharArrayOnCustomType()
    {
        var buf = new MyBuffer();
        foreach (char c in buf.ToCharArray()) // ok — not string.ToCharArray
            System.Console.WriteLine(c);
    }

    public static void Negative_ToCharArrayWithArguments()
    {
        string s = "test";
        foreach (char c in s.ToCharArray(0, 2)) // ok — overload with args
            System.Console.WriteLine(c);
    }

    private static string GetText() => "test";
    private static char[] GetChars() => ['a', 'b'];

    private sealed class MyBuffer
    {
        public char[] ToCharArray() => ['x'];
    }
}
