using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Creedengo.Sandbox;

// Mirrors GCI6605.UseExistsInsteadOfAny.Tests.cs.
// Lines marked "warns" should light up GCI6605 in the IDE; "ok" lines should stay clean.
internal static class GCI6605Sandbox
{
    public static bool Positive_LambdaOnList()
    {
        var list = new List<int> { 1, 2, 3 };
        return list.Any(x => x > 1); // warns
    }

    public static bool Positive_MethodGroupOnList()
    {
        var list = new List<string> { "a", "b" };
        return list.Any(string.IsNullOrEmpty); // warns
    }

    public static bool Positive_DerivedList()
    {
        var list = new MyList<int> { 1, 2, 3 };
        return list.Any(x => x > 1); // warns
    }

    public static void Positive_InCondition()
    {
        var list = new List<int> { 1, 2, 3 };
        if (list.Any(x => x == 2)) // warns
            Console.WriteLine("found");
    }

    public static bool Positive_ExplicitTypeArgument()
    {
        var list = new List<int> { 1, 2, 3 };
        return list.Any<int>(x => x > 1); // warns
    }

    public static bool Negative_ParameterlessAny()
    {
        var list = new List<int> { 1, 2, 3 };
        return list.Any(); // ok — parameterless
    }

    public static void Negative_NonListTypes()
    {
        var enumerable = Enumerable.Range(0, 10);
        _ = enumerable.Any(x => x > 5); // ok — IEnumerable, no Exists

        var set = new HashSet<int> { 1, 2, 3 };
        _ = set.Any(x => x > 1); // ok

        var dic = new Dictionary<int, string> { { 1, "a" } };
        _ = dic.Any(kv => kv.Key > 0); // ok
    }

    public static void Negative_InExpressionTree()
    {
        var list = new List<int> { 1, 2, 3 };
        Expression<Func<bool>> expr = () => list.Any(x => x > 1); // ok — expression tree
    }

    public static bool Negative_CustomAnyMethod()
    {
        var coll = new MyCollection<int> { 1, 2, 3 };
        return coll.Any(x => x > 1); // ok — not the LINQ extension
    }

    public static bool Negative_OnArray()
    {
        var arr = new int[] { 1, 2, 3 };
        return arr.Any(x => x > 1); // ok — array, no Exists method
    }

    private sealed class MyList<T> : List<T>;

    private sealed class MyCollection<T> : List<T>
    {
        public bool Any(Func<T, bool> predicate) => true;
    }
}
