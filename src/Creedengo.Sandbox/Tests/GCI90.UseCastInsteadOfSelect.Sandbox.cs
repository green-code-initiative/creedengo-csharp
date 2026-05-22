using System.Collections.Generic;
using System.Linq;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI90.UseCastInsteadOfSelect.Tests.cs.
// Lines marked "warns" should light up GCI90 in the IDE; "ok" lines should stay clean.
internal static class GCI90Sandbox
{
    public class BaseType { }
    public class DerivedType : BaseType { }

    public static IEnumerable<object> Positive_Simple(IEnumerable<string> values) =>
        values.Select(i => (object)i); // warns

    public static IEnumerable<object?> Positive_Nullable(IEnumerable<string?> values) =>
        values.Select(i => (object?)i); // warns

    public static void Positive_MultipleShapes(IEnumerable<DerivedType> derived)
    {
        _ = derived.Select(dt => (BaseType)dt); // warns
        _ = derived.Select(dt => (BaseType?)dt); // warns

        _ = derived.Select(i => (object)i); // warns
        _ = derived.Select(i => (object?)i); // warns

        _ = derived.Select<DerivedType, object>(i => i); // warns
        _ = derived.Select<DerivedType, object?>(i => i); // warns

        _ = Enumerable.Range(0, 1).Select<int, object>(i => i); // warns
        _ = Enumerable.Range(0, 1).Select<int, object?>(i => i); // warns
    }

    public static void Negative_AlreadyCast(IEnumerable<DerivedType> derived)
    {
        _ = derived.Cast<BaseType>(); // ok
        _ = derived.Cast<object>(); // ok
        _ = Enumerable.Range(0, 1).Cast<object>(); // ok
    }
}
