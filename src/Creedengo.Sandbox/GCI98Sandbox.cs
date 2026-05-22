using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Creedengo.Sandbox;

// Mirrors GCI98.UseThenByInsteadOfOrderBy.Tests.cs.
// Lines marked "warns" should light up GCI98 in the IDE; "ok" lines should stay clean.
internal static class GCI98Sandbox
{
    public static void Positive_OrderByAfterOrderBy()
    {
        var items = new List<(int A, int B)>();
        var query1 = items.OrderBy(x => x.A).OrderBy(x => x.B); // warns on second OrderBy
        var query2 = items
            .OrderBy(x => x.A)
            .OrderBy(x => x.B); // warns
    }

    public static void Positive_OrderByDescendingAfterOrderBy()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderBy(x => x.A).OrderByDescending(x => x.B); // warns
    }

    public static void Positive_OrderByAfterOrderByDescending()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderByDescending(x => x.A).OrderBy(x => x.B); // warns
    }

    public static void Positive_OrderByDescendingAfterOrderByDescending()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderByDescending(x => x.A).OrderByDescending(x => x.B); // warns
    }

    public static void Positive_OrderByAfterThenBy()
    {
        var items = new List<(int A, int B, int C)>();
        var result = items.OrderBy(x => x.A).ThenBy(x => x.B).OrderBy(x => x.C); // warns on final OrderBy
    }

    public static void Positive_Chained()
    {
        var items = new List<(int A, int B, int C)>();
        var result = items.OrderBy(x => x.A).OrderBy(x => x.B).OrderBy(x => x.C); // warns twice
    }

    public static void Negative_SingleOrderBy()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderBy(x => x.A); // ok
    }

    public static void Negative_SingleOrderByDescending()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderByDescending(x => x.A); // ok
    }

    public static void Negative_OrderByThenBy()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderBy(x => x.A).ThenBy(x => x.B); // ok — proper use
    }

    public static void Negative_OrderByFollowedBySelect()
    {
        var items = new List<(int A, int B)>();
        var result = items.OrderBy(x => x.A).Select(x => x.B); // ok
    }

    public static void Negative_InExpressionTree()
    {
        var items = new List<(int A, int B)>();
        Expression<System.Func<IEnumerable<(int A, int B)>>> expr =
            () => items.OrderBy(x => x.A).OrderBy(x => x.B); // ok — expression tree excluded
    }
}
