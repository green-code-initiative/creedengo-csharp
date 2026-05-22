using System;
using System.Collections.Generic;
using System.Linq;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI87.UseListIndexer.Tests.cs.
// Lines marked "warns" should light up GCI87 in the IDE; "ok" lines should stay clean.
internal static class GCI87Sandbox
{
    private sealed class MyCollection<T> : List<T>;

    private sealed class MyCollectionWithFirst<T> : List<T>
    {
        public T First() => this[0];
        public T Last() => this[^1];
        public T ElementAt(int index) => this[index];
    }

    public static void Positive_First()
    {
        var arr = new int[] { 1, 2, 3 };
        Console.WriteLine(arr.First()); // warns

        var list = new List<int> { 1, 2, 3 };
        Console.WriteLine(list.First()); // warns

        var coll = new MyCollection<int> { 1, 2, 3 };
        Console.WriteLine(coll.First()); // warns
    }

    public static int Negative_FirstWithNoIndexer()
    {
        var enumerable = Enumerable.Range(0, 10);
        return enumerable.First(); // ok — IEnumerable<T> has no indexer
    }

    public static void Negative_FirstWithPredicate()
    {
        var arr = new int[] { 1, 2, 3 };
        Console.WriteLine(arr.First(x => x != 1)); // ok — predicate overload

        var list = new List<int> { 1, 2, 3 };
        Console.WriteLine(list.First(x => x != 1)); // ok
    }

    public static void Negative_FirstUserDefined()
    {
        var coll = new MyCollectionWithFirst<int> { 1, 2, 3 };
        Console.WriteLine(coll.First()); // ok — calls user-defined member, not Enumerable.First
    }

    public static void Negative_FirstOnSetOrDictionary()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Console.WriteLine(set.First()); // ok — no indexer

        var dic = new Dictionary<int, int> { { 1, 1 } };
        Console.WriteLine(dic.First()); // ok
    }

    public static void Positive_Last()
    {
        var arr = new int[] { 1, 2, 3 };
        Console.WriteLine(arr.Last()); // warns

        var list = new List<int> { 1, 2, 3 };
        Console.WriteLine(list.Last()); // warns

        var coll = new MyCollection<int> { 1, 2, 3 };
        Console.WriteLine(coll.Last()); // warns
    }

    public static void Negative_LastWithPredicate()
    {
        var arr = new int[] { 1, 2, 3 };
        Console.WriteLine(arr.Last(x => x != 1)); // ok
    }

    public static void Negative_LastUserDefined()
    {
        var coll = new MyCollectionWithFirst<int> { 1, 2, 3 };
        Console.WriteLine(coll.Last()); // ok
    }

    public static void Positive_ElementAt()
    {
        var arr = new int[] { 1, 2, 3 };
        Console.WriteLine(arr.ElementAt(2)); // warns

        var list = new List<int> { 1, 2, 3 };
        Console.WriteLine(list.ElementAt(2)); // warns

        var coll = new MyCollection<int> { 1, 2, 3 };
        Console.WriteLine(coll.ElementAt(2)); // warns
    }

    public static void Negative_ElementAtWithNoIndexer()
    {
        var enumerable = Enumerable.Range(0, 10);
        Console.WriteLine(enumerable.ElementAt(2)); // ok
    }

    public static void Negative_ElementAtUserDefined()
    {
        var coll = new MyCollectionWithFirst<int> { 1, 2, 3 };
        Console.WriteLine(coll.ElementAt(2)); // ok
    }

    public static void Negative_ElementAtNonLists()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Console.WriteLine(set.ElementAt(2)); // ok

        var dic = new Dictionary<int, int> { { 1, 1 } };
        Console.WriteLine(dic.ElementAt(2)); // ok
    }
}
