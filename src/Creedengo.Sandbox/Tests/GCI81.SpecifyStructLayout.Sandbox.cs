using System;
using System.Runtime.InteropServices;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI81.SpecifyStructLayout.Tests.cs.
// Types marked "warns" should light up GCI81 in the IDE; "ok" types should stay clean.
internal static class GCI81Sandbox
{
    // ok — empty struct, no fields/props for which to specify layout
    public record struct GCI81_EmptyStruct;

    // ok — a single value-typed property
    public record struct GCI81_ValueProp(int A);

    // ok — single reference-typed property
    public record struct GCI81_ReferenceProp(string A);

    // ok — mix of value + reference (auto-layout doesn't improve anything)
    public record struct GCI81_MixedProps(int A, string B);

    // ok — already has [StructLayout(LayoutKind.Auto)]
    [StructLayout(LayoutKind.Auto)]
    public record struct GCI81_ExplicitLayout(int A, double B);

    // warns — multiple value-typed properties without layout
    public struct GCI81_TwoValueProps
    {
        public int A { get; set; }
        public double B { get; set; }
    }

    // warns — three+ value-typed properties without layout
    public struct GCI81_ThreeValueProps
    {
        public int A { get; set; }
        public double B { get; set; }
        public int C { get; set; }
    }

    // warns — record struct with multiple value-typed fields
    public record struct GCI81_ManyValueFields(bool A, int B, char C, short D, ulong E, DateTime F);
}
