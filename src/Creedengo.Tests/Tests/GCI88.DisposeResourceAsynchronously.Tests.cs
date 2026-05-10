namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class DisposeResourceAsynchronouslyTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<DisposeResourceAsynchronously, DisposeResourceAsynchronouslyFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontWarnOnMissingUsingsAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static async Task Run()
            {
                var d1 = new DisposableClass();
                Console.WriteLine(d1);

                var d2 = new AsyncDisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnNonAsyncableUsingsAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }

            public static async Task Run()
            {
                using (var d1 = new DisposableClass())
                    Console.WriteLine(d1);

                using var d2 = new DisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnAsyncableUsingsInAsyncMethod1Async() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static async Task Run()
            {
                // Leading trivia
                [|using|] (var d1 = new AsyncDisposableClass()) // Trailing trivia
                    Console.WriteLine(d1);

                // Leading trivia
                [|using|] var d2 = new AsyncDisposableClass(); // Trailing trivia
                Console.WriteLine(d2);

                // More tests to make sure everything stays in place
                Console.WriteLine();
        
                // Leading trivia
                [|using|] (var d3 = new AsyncDisposableClass()) // Trailing trivia
                {
                    Console.WriteLine(d3);
                    Console.WriteLine(d3);
                    Console.WriteLine(d3);
                }
            }
        }
        """, """
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static async Task Run()
            {
                // Leading trivia
                await using (var d1 = new AsyncDisposableClass()) // Trailing trivia
                    Console.WriteLine(d1);

                // Leading trivia
                await using var d2 = new AsyncDisposableClass(); // Trailing trivia
                Console.WriteLine(d2);

                // More tests to make sure everything stays in place
                Console.WriteLine();
        
                // Leading trivia
                await using (var d3 = new AsyncDisposableClass()) // Trailing trivia
                {
                    Console.WriteLine(d3);
                    Console.WriteLine(d3);
                    Console.WriteLine(d3);
                }
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnAsyncableUsingsInNonAsyncMethodAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private class DisposableClass : IDisposable { public void Dispose() { } }
            private sealed class AsyncDisposableClass : DisposableClass, IAsyncDisposable { public ValueTask DisposeAsync() => default; }

            public static void Run()
            {
                using (var d1 = new AsyncDisposableClass())
                    Console.WriteLine(d1);

                using var d2 = new AsyncDisposableClass();
                Console.WriteLine(d2);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnAsyncableUsingsInAsyncLocalFunctionAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static void Caller()
            {
                async Task Inner()
                {
                    [|using|] var d = new AsyncDisposableClass();
                    Console.WriteLine(d);
                    await Task.Yield();
                }
                _ = Inner();
            }
        }
        """, """
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static void Caller()
            {
                async Task Inner()
                {
                    await using var d = new AsyncDisposableClass();
                    Console.WriteLine(d);
                    await Task.Yield();
                }
                _ = Inner();
            }
        }
        """);

    [TestMethod]
    public Task WarnOnAsyncableUsingsInAsyncLambdaAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static void Caller()
            {
                Func<Task> f = async () =>
                {
                    [|using|] var d = new AsyncDisposableClass();
                    Console.WriteLine(d);
                    await Task.Yield();
                };
                _ = f();
            }
        }
        """, """
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static void Caller()
            {
                Func<Task> f = async () =>
                {
                    await using var d = new AsyncDisposableClass();
                    Console.WriteLine(d);
                    await Task.Yield();
                };
                _ = f();
            }
        }
        """);

    [TestMethod]
    public Task WarnOnAsyncableUsingsInAsyncAnonymousMethodAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static void Caller()
            {
                Func<Task> f = async delegate
                {
                    [|using|] var d = new AsyncDisposableClass();
                    Console.WriteLine(d);
                    await Task.Yield();
                };
                _ = f();
            }
        }
        """, """
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static void Caller()
            {
                Func<Task> f = async delegate
                {
                    await using var d = new AsyncDisposableClass();
                    Console.WriteLine(d);
                    await Task.Yield();
                };
                _ = f();
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnAsyncableUsingsInNonAsyncLambdaInsideAsyncMethodAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Test
        {
            private sealed class AsyncDisposableClass : IDisposable, IAsyncDisposable { public void Dispose() { } public ValueTask DisposeAsync() => default; }

            public static async Task Caller()
            {
                Action a = () =>
                {
                    using var d = new AsyncDisposableClass(); // Lambda is not async — should NOT be flagged.
                    Console.WriteLine(d);
                };
                a();
                await Task.Yield();
            }
        }
        """);
}
