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
    public Task StaticRegexMatchShouldReportAndFixAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                var m = [|Regex.Match("abc", @"\w")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");

            public void Run()
            {
                var m = _regex.Match("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexReplaceShouldReportAndFixAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                string r = [|Regex.Replace("abc", @"\w", "x")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");

            public void Run()
            {
                string r = _regex.Replace("abc", "x");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexSplitShouldReportAndFixAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                string[] r = [|Regex.Split("abc", @"\w")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");

            public void Run()
            {
                string[] r = _regex.Split("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexMatchesShouldReportAndFixAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                var r = [|Regex.Matches("abc", @"\w")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");

            public void Run()
            {
                var r = _regex.Matches("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexInStaticMethodShouldReportWithStaticFieldAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public static void Run()
            {
                bool isMatch = [|Regex.IsMatch("abc", @"\w")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private static readonly Regex _regex = new Regex(@"\w");

            public static void Run()
            {
                bool isMatch = _regex.IsMatch("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexInPropertyShouldReportAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public bool IsValid => [|Regex.IsMatch("abc", @"\w")|];
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w");

            public bool IsValid => _regex.IsMatch("abc");
        }
        """);

    [TestMethod]
    public Task StaticRegexWithNonConstantPatternShouldReportButNotFixAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run(string pattern)
            {
                bool isMatch = [|Regex.IsMatch("abc", pattern)|];
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexWithOptionsShouldReportAndFixAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                bool isMatch = [|Regex.IsMatch("abc", @"\w", RegexOptions.IgnoreCase)|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w", RegexOptions.IgnoreCase);

            public void Run()
            {
                bool isMatch = _regex.IsMatch("abc");
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexWithTimeoutShouldReportAndFixAsync() => VerifyAsync("""
        using System;
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                bool isMatch = [|Regex.IsMatch("abc", @"\w", RegexOptions.None, TimeSpan.FromSeconds(1))|];
            }
        }
        """, """
        using System;
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\w", RegexOptions.None, TimeSpan.FromSeconds(1));

            public void Run()
            {
                bool isMatch = _regex.IsMatch("abc");
            }
        }
        """);

    [TestMethod]
    public Task FullyQualifiedStaticRegexWithoutUsingShouldReportAndFixAsync() => VerifyAsync("""
        public class Test
        {
            public void Run()
            {
                bool isMatch = [|System.Text.RegularExpressions.Regex.IsMatch("abc", @"\w")|];
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
    public Task StaticRegexInStaticLocalFunctionShouldUseStaticFieldAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            public void Run()
            {
                static bool Check() => [|Regex.IsMatch("abc", @"\w")|];
                _ = Check();
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private static readonly Regex _regex = new Regex(@"\w");

            public void Run()
            {
                static bool Check() => _regex.IsMatch("abc");
                _ = Check();
            }
        }
        """);

    [TestMethod]
    public Task StaticRegexWhenRegexFieldAlreadyExistsShouldUseUniqueNameAsync() => VerifyAsync("""
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\d");
            public void Run()
            {
                bool isMatch = [|Regex.IsMatch("abc", @"\w")|];
            }
        }
        """, """
        using System.Text.RegularExpressions;
        public class Test
        {
            private readonly Regex _regex = new Regex(@"\d");
            private readonly Regex _regex1 = new Regex(@"\w");

            public void Run()
            {
                bool isMatch = _regex1.IsMatch("abc");
            }
        }
        """);
}
