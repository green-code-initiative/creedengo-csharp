using System;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI86.GCCollectShouldNotBeCalled.Tests.cs.
// Lines marked "warns" should light up GCI86 in the IDE; "ok" lines should stay clean.
internal static class GCI86Sandbox
{
    public static void Positive_PlainCall()
    {
        GC.Collect(); // warns
    }

    public static void Positive_FullyQualified()
    {
        System.GC.Collect(); // warns
    }

    public static void Positive_NamedArguments()
    {
        GC.Collect(mode: GCCollectionMode.Optimized, generation: 1); // warns
    }

    public static void Positive_MidStatement()
    {
        string text = ""; GC.Collect(); string text2 = ""; // warns
        _ = (text, text2);
    }

    public static void Negative_Commented()
    {
        //GC.Collect(); // ok — commented out
    }

    public static void Negative_Generation0()
    {
        GC.Collect(0); // ok — collecting generation 0 only is acceptable
    }

    public static void Positive_Generation10()
    {
        GC.Collect(10); // warns — out-of-range generation, effectively a full collection
    }

    public static void Negative_Generation0WithMode()
    {
        GC.Collect(0, GCCollectionMode.Forced); // ok
    }

    public static void Positive_Generation10WithMode()
    {
        GC.Collect(10, GCCollectionMode.Forced); // warns
    }
}
