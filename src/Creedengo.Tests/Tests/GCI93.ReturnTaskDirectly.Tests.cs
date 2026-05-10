namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class ReturnTaskDirectlyTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<ReturnTaskDirectly, ReturnTaskDirectlyFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontWarnWhenReturningTask1Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run() => Task.Delay(0);
        }
        """);

    [TestMethod]
    public Task DontWarnWhenReturningTask2Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run()
            {
                return Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnWhenReturningTask3Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run()
            {
                System.Console.WriteLine();
                return Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnWithMultipleStatements1Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static async Task Run()
            {
                System.Console.WriteLine();
                await Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnWithMultipleStatements2Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static async Task Run()
            {
                await Task.Delay(0);
                await Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitExpressionAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run() => await Task.Delay(0).ConfigureAwait(false);
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run() => Task.Delay(0);
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitWithTrivia1ExpressionAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run() => await Task.Delay(0).ConfigureAwait(false); // Comment
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run() => Task.Delay(0); // Comment
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitWithTrivia2ExpressionAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run() =>
                // Comment
                await Task.Delay(0);
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run() =>
                // Comment
                Task.Delay(0);
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitBody1Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run()
            {
                await Task.Delay(0).ConfigureAwait(true);
            }
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run()
            {
                return Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitBody2Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task<int> Run()
            {
                return await Task.FromResult(0);
            }
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task<int> Run()
            {
                return Task.FromResult(0);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitBodyWithTrivia1Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task Run()
            {
                // Comment 0
                await Task.Delay(0); // Comment 1
                // Comment 2
            }
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task Run()
            {
                // Comment 0
                return Task.Delay(0); // Comment 1
                // Comment 2
            }
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitBodyWithTrivia2Async() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static [|async|] Task<int> Run()
            {
                // Comment 0
                return await Task.FromResult(0).ConfigureAwait(false); // Comment 1
                // Comment 2
            }
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static Task<int> Run()
            {
                // Comment 0
                return Task.FromResult(0); // Comment 1
                // Comment 2
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnNestedAwaitExpressionAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static async Task Run() => await Task.Delay(await Task.FromResult(0));
        }
        """);

    [TestMethod]
    public Task DontWarnOnNestedAwaitBodyAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static async Task Run()
            {
                await Task.Delay(await Task.FromResult(0));
            }
        }
        """);

    [TestMethod]
    public Task DontWarnWhenReturnTypeIsValueTaskAndAwaitedExpressionIsTaskAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static async ValueTask DisposeAsync()
            {
                await Task.Delay(0);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnWhenReturnTypeIsValueTaskTAndAwaitedExpressionIsTaskTAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static async ValueTask<int> Run() => await Task.FromResult(0);
        }
        """);

    [TestMethod]
    public Task DontWarnWhenReturnTypeIsTaskAndAwaitedExpressionIsValueTaskAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static ValueTask GetValueTask() => default;
            public static async Task Run() => await GetValueTask();
        }
        """);

    [TestMethod]
    public Task DontWarnWhenReturnTypeIsTaskTAndAwaitedExpressionIsValueTaskTAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static ValueTask<int> GetValueTask() => default;
            public static async Task<int> Run() => await GetValueTask();
        }
        """);

    [TestMethod]
    public Task WarnWhenReturnTypeAndAwaitedExpressionAreBothValueTaskAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static ValueTask GetValueTask() => default;
            public static [|async|] ValueTask Run() => await GetValueTask().ConfigureAwait(false);
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static ValueTask GetValueTask() => default;
            public static ValueTask Run() => GetValueTask();
        }
        """);

    [TestMethod]
    public Task WarnWhenReturnTypeAndAwaitedExpressionAreBothValueTaskTAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static ValueTask<int> GetValueTask() => default;
            public static [|async|] ValueTask<int> Run() => await GetValueTask();
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static ValueTask<int> GetValueTask() => default;
            public static ValueTask<int> Run() => GetValueTask();
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitInLocalFunctionExpressionAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static void Caller()
            {
                [|async|] Task Inner() => await Task.Delay(0).ConfigureAwait(false);
                _ = Inner();
            }
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static void Caller()
            {
                Task Inner() => Task.Delay(0);
                _ = Inner();
            }
        }
        """);

    [TestMethod]
    public Task WarnOnSingleAwaitInLocalFunctionBodyAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static void Caller()
            {
                [|async|] Task<int> Inner()
                {
                    return await Task.FromResult(0);
                }
                _ = Inner();
            }
        }
        """, """
        using System.Threading.Tasks;
        public static class Test
        {
            public static void Caller()
            {
                Task<int> Inner()
                {
                    return Task.FromResult(0);
                }
                _ = Inner();
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnLocalFunctionWithMismatchedTaskKindAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Test
        {
            public static void Caller()
            {
                async ValueTask Inner() => await Task.Delay(0);
                _ = Inner();
            }
        }
        """);
}
