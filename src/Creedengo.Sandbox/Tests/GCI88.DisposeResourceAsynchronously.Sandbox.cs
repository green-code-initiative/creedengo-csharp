using System;
using System.Threading.Tasks;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI88.DisposeResourceAsynchronously.Tests.cs.
// 'using' tokens marked "warns" should light up GCI88 in the IDE; "ok" usings should stay clean.
internal static class GCI88Sandbox
{
    private class DisposableOnly : IDisposable { public void Dispose() { } }
    private sealed class AsyncDisposable : DisposableOnly, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

    public static async Task Positive_AsyncableUsingsInAsyncMethod()
    {
        using (var d1 = new AsyncDisposable()) // warns
            Console.WriteLine(d1);

        using var d2 = new AsyncDisposable(); // warns
        Console.WriteLine(d2);

        using (var d3 = new AsyncDisposable()) // warns
        {
            Console.WriteLine(d3);
        }

        await Task.Yield();
    }

    public static void Negative_AsyncableUsingsInNonAsyncMethod()
    {
        using (var d1 = new AsyncDisposable()) // ok — outer method is not async
            Console.WriteLine(d1);

        using var d2 = new AsyncDisposable(); // ok
        Console.WriteLine(d2);
    }

    public static async Task Negative_NonAsyncableUsings()
    {
        using (var d1 = new DisposableOnly()) // ok — not IAsyncDisposable
            Console.WriteLine(d1);

        using var d2 = new DisposableOnly(); // ok
        Console.WriteLine(d2);

        await Task.Yield();
    }

    public static void Positive_InAsyncLocalFunction()
    {
        async Task Inner()
        {
            using var d = new AsyncDisposable(); // warns
            Console.WriteLine(d);
            await Task.Yield();
        }
        _ = Inner();
    }

    public static void Positive_InAsyncLambda()
    {
        Func<Task> f = async () =>
        {
            using var d = new AsyncDisposable(); // warns
            Console.WriteLine(d);
            await Task.Yield();
        };
        _ = f();
    }

    public static void Positive_InAsyncAnonymousMethod()
    {
        Func<Task> f = async delegate
        {
            using var d = new AsyncDisposable(); // warns
            Console.WriteLine(d);
            await Task.Yield();
        };
        _ = f();
    }

    public static async Task Negative_NonAsyncLambdaInsideAsyncMethod()
    {
        Action a = () =>
        {
            using var d = new AsyncDisposable(); // ok — the lambda itself is not async
            Console.WriteLine(d);
        };
        a();
        await Task.Yield();
    }
}
