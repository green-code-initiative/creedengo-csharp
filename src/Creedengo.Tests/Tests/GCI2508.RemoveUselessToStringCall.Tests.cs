namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class RemoveUselessToStringCallTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<RemoveUselessToStringCall>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task ShouldNotCallToStringOnAStringAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                Console.Write([|str.ToString()|]);
            }
        }
        """);

    [TestMethod]
    public Task SimpleAssignmentAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                [|str.ToString()|];
            }
        }
        """);

    [TestMethod]
    public Task AssignedToVariableAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                string str2 = str.ToString();
            }
        }
        """);
    [TestMethod]
    public Task DiscardIsReportedAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                _ = [|str.ToString()|];
            }
        }
        """);

    [TestMethod]
    public Task OverloadsAreAllowedAsync() => VerifyAsync("""
        using System;
        using System.Globalization;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                _ = str.ToString(CultureInfo.CurrentCulture);
            }
        }
        """);

    [TestMethod]
    public Task NullSemanticsAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                _ = str?.ToString();
            }
        }
        """);

    [TestMethod]
    public Task CastToObjectAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                string str = "a";
                ((object)str).ToString();
            }
        }
        """);

    [TestMethod]
    public Task NonStringsPassAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            public static void Main()
            {
                int i = 3;
                _ = i.ToString();
            }
        }
        """);
}
