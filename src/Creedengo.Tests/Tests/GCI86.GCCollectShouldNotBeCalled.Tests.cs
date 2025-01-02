namespace Creedengo.Tests.Tests;

[TestClass]
public class GCCollectShouldNotBeCalledTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<GCCollectShouldNotBeCalled>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task GCCollectShouldNotBeCalledAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect()|];
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledSystemAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|System.GC.Collect()|];
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledNamedArgumentsAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect(mode: GCCollectionMode.Optimized, generation: 1)|];
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledMultipleCodeAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                string text="";       [|GC.Collect()|]; string text2 = "";
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledCommentedAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                //GC.Collect();
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledGeneration0Async() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                GC.Collect(0);
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledGeneration10Async() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect(10)|];
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledGeneratio0CollectionModeAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                GC.Collect(0, GCCollectionMode.Forced);
            }
        }
        """);

    [TestMethod]
    public Task GCCollectShouldNotBeCalledGeneration10CollectionModeAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static async void Main()
            {
                [|GC.Collect(10, GCCollectionMode.Forced)|];
            }
        }
        """);
}
