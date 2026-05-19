namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class RemoveRedundantToCharArrayCallTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<RemoveRedundantToCharArrayCall, RemoveRedundantToCharArrayCallFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task ToCharArrayCallShouldBeRemovedAsync() => VerifyAsync("""
        public class Test
        {
            public void Run()
            {
                string s = "test";

                foreach (char c in s.[|ToCharArray|]())
                    System.Console.WriteLine(c);
            }
        }
        """,
        """
        public class Test
        {
            public void Run()
            {
                string s = "test";

                foreach (char c in s)
                    System.Console.WriteLine(c);
            }
        }
        """);
}
