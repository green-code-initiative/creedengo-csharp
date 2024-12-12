namespace Creedengo.Tests;

[TestClass]
public sealed class AvoidAsyncVoidMethodsTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<AvoidAsyncVoidMethods, AvoidAsyncVoidMethodsFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task AvoidAsyncVoidMethodAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Program
        {
            public static async void [|Main|]()
            {
                await Task.Delay(1000);
                Console.WriteLine();
            }
        }
        """, """
        using System;
        using System.Threading.Tasks;
        public static class Program
        {
            public static async Task Main()
            {
                await Task.Delay(1000);
                Console.WriteLine();
            }
        }
        """);

    [TestMethod]
    public Task AvoidAsyncVoidMethodWithMissingUsingAsync() => VerifyAsync("""
        using System;
        using System.Net.Http;
        public static class Program
        {
            public static async void [|Main|]()
            {
                using var httpClient = new HttpClient();
                _ = await httpClient.GetAsync(new Uri("URL"));
            }
        }
        """, """
        using System;
        using System.Net.Http;
        public static class Program
        {
            public static async {|CS0246:Task|} {|CS0161:Main|}()
            {
                using var httpClient = new HttpClient();
                _ = await httpClient.GetAsync(new Uri("URL"));
            }
        }
        """);

    [TestMethod]
    public Task AsyncTaskMethodIsOkAsync() => VerifyAsync("""
        using System;
        using System.Threading.Tasks;
        public static class Program
        {
            public static async Task Main()
            {
                await Task.Delay(1000);
                Console.WriteLine();
            }
        }
        """);

    [TestMethod]
    public Task AsyncGenericTaskMethodIsOkAsync() => VerifyAsync("""
        using System.Threading.Tasks;
        public static class Program
        {
            public static async Task<int> Main()
            {
                await Task.Delay(1000);
                return 1;
            }
        }
        """);
}
