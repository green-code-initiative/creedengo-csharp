using System;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI83.ReplaceEnumToStringWithNameOf.Tests.cs.
// Lines marked "warns" should light up GCI83 in the IDE; "ok" lines should stay clean.
internal static class GCI83Sandbox
{
    private enum MyEnum { A, B, C, D }

    public static void Positive_ToStringWithDefaultOrEquivalentFormat()
    {
        Console.WriteLine(MyEnum.A.ToString()); // warns
        Console.WriteLine(MyEnum.B.ToString("")); // warns
        Console.WriteLine(MyEnum.C.ToString(string.Empty)); // warns
        Console.WriteLine(MyEnum.D.ToString(format: null)); // warns
    }

    public static void Positive_FormatGorF()
    {
        Console.WriteLine(MyEnum.A.ToString("G")); // warns — "G" is the default format
        Console.WriteLine(MyEnum.B.ToString("F")); // warns — "F" is equivalent to default for non-flags enums
    }

    public static void Negative_NumericFormat()
    {
        Console.WriteLine(MyEnum.C.ToString("N")); // ok — "N" prints the numeric value, not the name
    }

    public static void Positive_InInterpolation()
    {
        Console.WriteLine($"{MyEnum.A}"); // warns
        Console.WriteLine($"{MyEnum.B:G}"); // warns
        Console.WriteLine($"{MyEnum.C:F}"); // warns
    }

    public static void Negative_InInterpolationNumericFormat()
    {
        Console.WriteLine($"{MyEnum.D:N}"); // ok
    }
}
