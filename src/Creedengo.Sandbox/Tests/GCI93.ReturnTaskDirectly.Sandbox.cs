using System;
using System.Threading.Tasks;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI93.ReturnTaskDirectly.Tests.cs.
// 'async' tokens marked "warns" should light up GCI93 in the IDE; "ok" methods should stay clean.
internal static class GCI93Sandbox
{
    public static Task Negative_ReturnsTaskExpressionBody() => Task.Delay(0); // ok

    public static Task Negative_ReturnsTaskBlockBody() // ok
    {
        return Task.Delay(0);
    }

    public static Task Negative_TaskAfterStatement() // ok
    {
        Console.WriteLine();
        return Task.Delay(0);
    }

    public static async Task Negative_MultipleStatements1() // ok — more than one statement
    {
        Console.WriteLine();
        await Task.Delay(0);
    }

    public static async Task Negative_MultipleStatements2() // ok
    {
        await Task.Delay(0);
        await Task.Delay(0);
    }

    public static async Task Positive_SingleAwaitExpressionBody() => // warns on async
        await Task.Delay(0).ConfigureAwait(false);

    public static async Task Positive_SingleAwaitExpressionBodyWithTrailingComment() => // warns
        await Task.Delay(0).ConfigureAwait(false); // Comment

    public static async Task Positive_SingleAwaitBlockBody() // warns
    {
        await Task.Delay(0).ConfigureAwait(true);
    }

    public static async Task<int> Positive_SingleAwaitReturn() // warns
    {
        return await Task.FromResult(0);
    }

    public static async Task Negative_NestedAwaitExpression() => // ok — nested await prevents direct return
        await Task.Delay(await Task.FromResult(0));

    public static async Task Negative_NestedAwaitBody() // ok
    {
        await Task.Delay(await Task.FromResult(0));
    }

    public static async ValueTask Negative_ReturnsValueTaskAwaitsTask() // ok — mismatch between Task and ValueTask
    {
        await Task.Delay(0);
    }

    public static async ValueTask<int> Negative_ReturnsValueTaskTAwaitsTaskT() => // ok
        await Task.FromResult(0);

    public static ValueTask GetValueTask() => default;
    public static ValueTask<int> GetValueTaskInt() => default;

    public static async Task Negative_ReturnsTaskAwaitsValueTask() => await GetValueTask(); // ok
    public static async Task<int> Negative_ReturnsTaskTAwaitsValueTaskT() => await GetValueTaskInt(); // ok

    public static async ValueTask Positive_BothValueTask() => // warns — return type and awaited type match
        await GetValueTask().ConfigureAwait(false);

    public static async ValueTask<int> Positive_BothValueTaskT() => await GetValueTaskInt(); // warns

    public static void Caller_LocalFunctions()
    {
        async Task Inner1() => await Task.Delay(0).ConfigureAwait(false); // warns on async

        async Task<int> Inner2() // warns
        {
            return await Task.FromResult(0);
        }

        async ValueTask Inner3() => await Task.Delay(0); // ok — mismatched task kind

        _ = Inner1();
        _ = Inner2();
        _ = Inner3();
    }
}
