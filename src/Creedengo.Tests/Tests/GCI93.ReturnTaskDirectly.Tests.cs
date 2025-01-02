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
}
