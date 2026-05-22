using System;
using System.IO;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI69.DontCallFunctionsInLoopConditions.Tests.cs.
// Lines marked "warns" should light up GCI69 in the IDE; "ok" lines should stay clean.
internal static class GCI69Sandbox
{
    private const int C = 10;
    private static int V1 => 10;
    private static int V2() => 10;
    private static int V3(int i) => i;

    public static void Positive_ForLoop(int p)
    {
        int j = 0, k = 10;

        for (int i = 0; i < V1 && i < V2() /*warns*/ && i < V3(i) && i < V3(j) && i < V3(k) /*warns*/ && i < V3(p) /*warns*/ && i < V3(C) /*warns*/; i++)
            j += i;

        for (int i = 0; i < V1 && i < V2() /*warns*/ && i < V3(i) && i < V3(j) && i < V3(k) /*warns*/ && i < V3(p) /*warns*/ && i < V3(C) /*warns*/; i++)
        {
            j += i;
        }
    }

    public static void Positive_WhileLoop(int p)
    {
        int i = 0, j = 0, k = 10;

        while (i < V1 && i < V2() /*warns*/ && i < V3(i) && i < V3(j) && i < V3(k) /*warns*/ && i < V3(p) /*warns*/ && i < V3(C) /*warns*/)
            j += i++;

        while (i < V1 && i < V2() /*warns*/ && i < V3(i) && i < V3(j) && i < V3(k) /*warns*/ && i < V3(p) /*warns*/ && i < V3(C) /*warns*/)
        {
            j += i++;
        }
    }

    public static void Positive_DoWhileLoop(int p)
    {
        int i = 0, j = 0, k = 10;

        do j += i++;
        while (i < V1 && i < V2() /*warns*/ && i < V3(i) && i < V3(j) && i < V3(k) /*warns*/ && i < V3(p) /*warns*/ && i < V3(C) /*warns*/);

        do
        {
            j += i++;
        } while (i < V1 && i < V2() /*warns*/ && i < V3(i) && i < V3(j) && i < V3(k) /*warns*/ && i < V3(p) /*warns*/ && i < V3(C) /*warns*/);
    }

    public static void Negative_LoopVariantNullables(string? path)
    {
        // 'path' is reassigned inside the loop — the condition operand changes each iteration, so calls on it aren't invariant.
        for (path = Path.GetDirectoryName(path); !path!.Equals(@"S:\", StringComparison.OrdinalIgnoreCase); path = Path.GetDirectoryName(path)) { }
        for (path = Path.GetDirectoryName(path); path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) != true; path = Path.GetDirectoryName(path)) { }

        while (!path!.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)) path = Path.GetDirectoryName(path);
        while (path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) != true) path = Path.GetDirectoryName(path);

        do path = Path.GetDirectoryName(path); while (!path!.Equals(@"S:\", StringComparison.OrdinalIgnoreCase));
        do path = Path.GetDirectoryName(path); while (path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) != true);
    }

    public static void Positive_LoopInvariantNullables(string? path)
    {
        // 'path' is only assigned outside / before the loop, so the calls in the condition are invariant.
        for (path = Path.GetDirectoryName(path); !path!.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) /*warns*/; ) { }
        for (path = Path.GetDirectoryName(path); path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) /*warns*/ != true; ) { }

        while (!path!.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) /*warns*/) ;
        while (path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) /*warns*/ != true) ;

        do ; while (!path!.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) /*warns*/);
        do ; while (path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) /*warns*/ != true);
    }
}
