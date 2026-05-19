namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseRegexInstanceInsteadOfStaticMethodTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseRegexInstanceInsteadOfStaticMethod, UseRegexInstanceInsteadOfStaticMethodFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task RegexInstanceCallShouldNotReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");
            public void Run()
            {
                bool isMatch = _regex.IsMatch("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexIsMatchShouldReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                bool isMatch = [|Regex.IsMatch("abc", @"\w")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");
        
            public void Run()
            {
                bool isMatch = _regex.IsMatch("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexMatchShouldReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                var m = [|Regex.Match("abc", @"\w")|];
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexReplaceShouldReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                string r = [|Regex.Replace("abc", @"\w", "x")|];
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexSplitShouldReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                string[] r = [|Regex.Split("abc", @"\w")|];
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexMatchesShouldReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                var r = [|Regex.Matches("abc", @"\w")|];
            }
        }
        """);
}
