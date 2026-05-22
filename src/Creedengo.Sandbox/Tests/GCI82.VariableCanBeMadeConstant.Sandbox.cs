using System;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI82.VariableCanBeMadeConstant.Tests.cs.
// Members marked "warns" should light up GCI82 in the IDE; "ok" members should stay clean.
internal static class GCI82Sandbox
{
    private const int AlreadyConstInt = 1; // ok
    private const string AlreadyConstString = "Bar"; // ok

    private static readonly int StaticReadonlyInt = 1; // warns — could be const
    private static readonly string StaticReadonlyString = "Bar"; // warns

    private static readonly object StaticReadonlyObject = new(); // ok — object can't be const
    private static readonly int StaticReadonlyNonConstantValue = Environment.TickCount; // ok — initializer isn't a compile-time constant

    private static readonly int MultiA = 1; // warns
    private static readonly string MultiB = "foo"; // warns

    private static readonly int StaticReadonlyUninitialized; // ok — no initializer

    public static void Positive_LocalIntCanBeConst()
    {
        int i = 0; // warns — could be const
        Console.WriteLine(i);
    }

    public static void Negative_LocalReassigned()
    {
        int i = 0; // ok — gets mutated
        Console.WriteLine(i++);
    }

    public static void Negative_AlreadyConst()
    {
        const int i = 0; // ok
        Console.WriteLine(i);
    }

    public static void Negative_NoInitializer()
    {
        int i;
        i = 0;
        Console.WriteLine(i);
    }

    public static void Negative_RuntimeInitializer()
    {
        int i = DateTime.Now.DayOfYear; // ok — runtime value
        Console.WriteLine(i);
    }

    public static void Negative_MultipleInitializersWithOneRuntime()
    {
        int i = 0, j = DateTime.Now.DayOfYear; // ok — j isn't a compile-time constant
        Console.WriteLine(i);
        Console.WriteLine(j);
    }

    public static void Positive_MultipleInitializersAllConst()
    {
        int i = 0, j = 0; // warns
        Console.WriteLine(i);
        Console.WriteLine(j);
    }

    public static void Negative_ObjectStringCannotBeConst()
    {
        object s = "abc"; // ok — declared type is object, not string
        Console.WriteLine(s);
    }

    public static void Positive_StringCanBeConst()
    {
        string s = "abc"; // warns
        Console.WriteLine(s);
    }

    public static void Positive_VarIntCanBeConst()
    {
        var item = 4; // warns
        Console.WriteLine(item);
    }

    public static void Positive_VarStringCanBeConst()
    {
        var item = "abc"; // warns
        Console.WriteLine(item);
    }

    public static void Positive_NullReferenceTypeInitializer()
    {
        string item = null!; // warns — const string item = null is valid
        Console.WriteLine(item);
    }

    public static void Negative_NullableValueTypeInitializer()
    {
        int? item = null; // ok — 'const int?' is not allowed (CS0283)
        Console.WriteLine(item);
    }

    public static void Negative_UserStructInitializer()
    {
        Point item = default; // ok — user-defined structs cannot be const
        Console.WriteLine(item);
    }

    private struct Point { public int X; }
}
