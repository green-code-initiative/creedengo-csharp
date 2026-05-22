using System.Collections.Generic;
using System.Linq;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI91.UseWhereBeforeOrderBy.Tests.cs.
// 'Where' / 'where' tokens marked "warns" should light up GCI91 in the IDE; "ok" lines should stay clean.
internal static class GCI91Sandbox
{
    public static void Negative_WhereOnly()
    {
        var items = new List<int>();
        var query = items.Where(x => x > 10).Select(x => x); // ok
    }

    public static void Negative_OrderByOnly()
    {
        var items = new List<int>();
        var query = items.OrderBy(x => x).Select(x => x); // ok
    }

    public static void Negative_OrderByDescendingOnly()
    {
        var items = new List<int>();
        var query = items.OrderByDescending(x => x).Select(x => x); // ok
    }

    public static void Negative_RightOrderMethod()
    {
        var items = new List<int>();
        var query = items.Where(x => x > 10).OrderBy(x => x).Select(x => x); // ok
    }

    public static void Positive_WrongOrderMethod()
    {
        var items = new List<int>();
        var query1 = items.OrderBy(x => x).Where(x => x > 10).Select(x => x); // warns on Where
        var query2 = items
            .OrderBy(x => x)
            .Where(x => x > 10) // warns
            .Select(x => x);
    }

    public static void Positive_WrongOrderDescendingMethod()
    {
        var items = new List<int>();
        var query = items.OrderByDescending(x => x).Where(x => x > 10).Select(x => x); // warns on Where
    }

    public static void Positive_WrongMultipleOrderMethod()
    {
        var items = new List<int>();
        var query = items.OrderBy(x => x).ThenByDescending(x => x).ThenBy(x => x).Where(x => x > 10).Select(x => x); // warns on Where
    }

    public static void Negative_WhereOnlyQuery()
    {
        var items = new List<int>();
        var query = from item in items
                    where item > 10
                    select item; // ok
    }

    public static void Negative_OrderByOnlyQuery()
    {
        var items = new List<int>();
        var query = from item in items
                    orderby item
                    select item; // ok
    }

    public static void Negative_RightOrderQuery()
    {
        var items = new List<int>();
        var query = from item in items
                    where item > 10
                    orderby item
                    select item; // ok
    }

    public static void Positive_WrongOrderQuery()
    {
        var items = new List<int>();
        var query = from item in items
                    orderby item
                    where item > 10 // warns
                    select item;
    }

    public static void Positive_WrongOrderDescendingQuery()
    {
        var items = new List<int>();
        var query = from item in items
                    orderby item descending
                    where item > 10 // warns
                    select item;
    }

    public static void Positive_WrongMultipleOrderQuery()
    {
        var items = new List<(int A, int B)>();
        var query = from item in items
                    orderby item.A
                    orderby item.B
                    where item.A > 10 // warns
                    select item;
    }

    public static void Negative_DisjointedOrderQuery()
    {
        var items = new List<int>();
        var query = from item in items
                    orderby item
                    select item
                    into item
                    where item > 0
                    select item; // ok — 'into' starts a new query
    }
}
