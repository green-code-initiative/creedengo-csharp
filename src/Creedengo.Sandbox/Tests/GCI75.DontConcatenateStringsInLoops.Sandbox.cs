using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI75.DontConcatenateStringsInLoops.Tests.cs.
// Lines marked "warns" should light up GCI75 in the IDE; "ok" lines should stay clean.
internal static class GCI75Sandbox
{
    private static string _staticField = string.Empty;
    private static string StaticProperty { get; set; } = string.Empty;

    public static void Positive_ParameterInAllLoops(string s)
    {
        for (int i = 0; i < 10; i++)
        {
            string si = i.ToString();
            s += si; // warns
            s = s + si; // warns
            s = si + s; // warns
            s = si + si; // ok — neither operand reads the running 's'
        }

        foreach (int i in Enumerable.Range(0, 10))
        {
            string si = i.ToString();
            s += si; // warns
            s = s + si; // warns
            s = si + s; // warns
        }

        int j = 0;
        while (j++ < 10)
        {
            string si = j.ToString();
            s += si; // warns
        }

        int k = 0;
        do
        {
            string si = k.ToString();
            s += si; // warns
        } while (++k < 10);
    }

    public static void Positive_LocalVariable()
    {
        string s = string.Empty;
        for (int i = 0; i < 10; i++)
        {
            string si = i.ToString();
            s += si; // warns
            s = s + si; // warns
            s = si + s; // warns
        }

        for (int i = 0; i < 10; i++)
        {
            string si = i.ToString();
            string s2 = string.Empty; // s2 is declared INSIDE the loop, so concatenating into it is fine
            s2 += si; // ok
            s2 = s2 + si; // ok
            s2 = si + s2; // ok
        }
    }

    public static void Positive_StaticField()
    {
        for (int i = 0; i < 10; i++)
        {
            string si = i.ToString();
            _staticField += si; // warns
        }
    }

    public static void Positive_StaticProperty()
    {
        for (int i = 0; i < 10; i++)
        {
            string si = i.ToString();
            StaticProperty += si; // warns
        }
    }

    public static void Positive_ListForEachAndParallelForEach(List<int> list, string s)
    {
        list.ForEach(i =>
        {
            string si = i.ToString();
            s += si; // warns
        });
        list.ForEach(i => s += i.ToString()); // warns
        list.ForEach(i => s = s + i.ToString()); // warns
        list.ForEach(i => s = i.ToString() + s); // warns
        list.ForEach(i => s = i.ToString() + i.ToString()); // ok

        Parallel.ForEach(list, i => s += i.ToString()); // warns
        Parallel.ForEach(list, i => s = s + i.ToString()); // warns
        Parallel.ForEach(list, i => s = i.ToString() + s); // warns
        Parallel.ForEach(list, i => s = i.ToString() + i.ToString()); // ok
    }

    public static void Positive_ImmutableListForEach(ImmutableList<int> list, string s)
    {
        list.ForEach(i => s += i.ToString()); // warns
        Parallel.ForEach(list, i => s += i.ToString()); // warns
    }
}
