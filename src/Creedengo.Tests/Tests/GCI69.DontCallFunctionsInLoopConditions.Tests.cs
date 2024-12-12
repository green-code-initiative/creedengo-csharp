namespace Creedengo.Tests;

[TestClass]
public sealed class DontCallFunctionsInLoopConditionsTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<DontCallFunctionsInLoopConditions>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task ForLoopFunctionCallShouldNotBeCalledAsync() => VerifyAsync("""
        public class Test
        {
            private const int C = 10;
            private static int V1 => 10;
            private static int V2() => 10;
            private static int V3(int i) => i;

            public void Run(int p)
            {
                int j = 0, k = 10;

                for (int i = 0; i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]; i++)
                    j += i;

                for (int i = 0; i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]; i++)
                {
                    j += i;
                }
            }
        }
        """);

    [TestMethod]
    public Task WhileLoopFunctionCallShouldNotBeCalledAsync() => VerifyAsync("""
        public class Test
        {
            private const int C = 10;
            private static int V1 => 10;
            private static int V2() => 10;
            private static int V3(int i) => i;

            public void Run(int p)
            {
                int i = 0, j = 0, k = 10;

                while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|])
                    j += i++;

                while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|])
                {
                    j += i++;
                }
            }
        }
        """);

    [TestMethod]
    public Task DoWhileLoopFunctionCallShouldNotBeCalledAsync() => VerifyAsync("""
        public class Test
        {
            private const int C = 10;
            private static int V1 => 10;
            private static int V2() => 10;
            private static int V3(int i) => i;

            public void Run(int p)
            {
                int i = 0, j = 0, k = 10;
                
                do j += i++;
                while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]);

                do
                {
                    j += i++;
                } while (i < V1 && i < [|V2()|] && i < V3(i) && i < V3(j) && i < [|V3(k)|] && i < [|V3(p)|] && i < [|V3(C)|]);
            }
        }
        """);

    [TestMethod]
    public Task WithLoopVariantNullablesAsync() => VerifyAsync("""
        using System;
        using System.IO;
        public class Test
        {
            public static void Run1(string? path)
            {
                for (path = Path.GetDirectoryName(path); !path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase); path = Path.GetDirectoryName(path)) { }
                for (path = Path.GetDirectoryName(path); path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) != true; path = Path.GetDirectoryName(path)) { }

                while (!path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)) path = Path.GetDirectoryName(path);
                while (path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) != true) path = Path.GetDirectoryName(path);

                do path = Path.GetDirectoryName(path); while (!path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase));
                do path = Path.GetDirectoryName(path); while (path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase) != true);
            }
        }
        """);

    [TestMethod]
    public Task WithLoopInvariantNullablesAsync() => VerifyAsync("""
        using System;
        using System.IO;
        public class Test
        {
            public static void Run(string? path)
            {
                for (path = Path.GetDirectoryName(path); ![|path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)|]; ) { }
                for (path = Path.GetDirectoryName(path); [|path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)|] != true; ) { }
        
                while (![|path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)|]) ;
                while ([|path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)|] != true) ;
        
                do ; while (![|path.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)|]);
                do ; while ([|path?.Equals(@"S:\", StringComparison.OrdinalIgnoreCase)|] != true);
            }
        }
        """);
}
