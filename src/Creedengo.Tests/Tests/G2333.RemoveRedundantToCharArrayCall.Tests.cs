namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class RemoveRedundantToCharArrayCallTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<RemoveRedundantToCharArrayCall, RemoveRedundantToCharArrayCallFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    // --- No-diagnostic cases ---

    [TestMethod] // foreach expression is not an invocation (plain variable) — first guard
    public Task ForeachOverPlainStringVariableNoDiagnosticAsync() => VerifyAsync("""
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

    [TestMethod] // foreach expression is not an invocation (plain char array) — first guard
    public Task ForeachOverCharArrayNoDiagnosticAsync() => VerifyAsync("""
        public class Test
        {
            public void Run()
            {
                char[] chars = new char[] { 'a', 'b' };

                foreach (char c in chars)
                    System.Console.WriteLine(c);
            }
        }
        """);

    [TestMethod] // invocation is not a member access (bare method call) — second guard
    public Task ForeachOverBareMethodCallNoDiagnosticAsync() => VerifyAsync("""
        public class Test
        {
            private static char[] GetChars() => new char[] { 'a', 'b' };

            public void Run()
            {
                foreach (char c in GetChars())
                    System.Console.WriteLine(c);
            }
        }
        """);

    [TestMethod] // method name is not "ToCharArray" — third guard
    public Task ForeachOverOtherStringMethodNoDiagnosticAsync() => VerifyAsync("""
        public class Test
        {
            public void Run()
            {
                string s = "hello world";

                foreach (string part in s.Split(' '))
                    System.Console.WriteLine(part);
            }
        }
        """);

    [TestMethod] // "ToCharArray" on a non-string type — fifth guard (ContainingType.SpecialType check)
    public Task ForeachOverToCharArrayOnCustomTypeNoDiagnosticAsync() => VerifyAsync("""
        public class MyBuffer
        {
            public char[] ToCharArray() => new char[] { 'x' };
        }

        public class Test
        {
            public void Run()
            {
                var buf = new MyBuffer();

                foreach (char c in buf.ToCharArray())
                    System.Console.WriteLine(c);
            }
        }
        """);

    // --- Positive cases (diagnostic + fix) ---

    [TestMethod]
    public Task ToCharArrayOnVariableShouldBeRemovedAsync() => VerifyAsync("""
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

    [TestMethod]
    public Task ToCharArrayOnStringLiteralShouldBeRemovedAsync() => VerifyAsync("""
        public class Test
        {
            public void Run()
            {
                foreach (char c in "hello".[|ToCharArray|]())
                    System.Console.WriteLine(c);
            }
        }
        """,
        """
        public class Test
        {
            public void Run()
            {
                foreach (char c in "hello")
                    System.Console.WriteLine(c);
            }
        }
        """);

    [TestMethod]
    public Task ToCharArrayOnMethodReturnValueShouldBeRemovedAsync() => VerifyAsync("""
        public class Test
        {
            private static string GetText() => "test";

            public void Run()
            {
                foreach (char c in GetText().[|ToCharArray|]())
                    System.Console.WriteLine(c);
            }
        }
        """,
        """
        public class Test
        {
            private static string GetText() => "test";

            public void Run()
            {
                foreach (char c in GetText())
                    System.Console.WriteLine(c);
            }
        }
        """);

    [TestMethod] // ToCharArray(int, int) overload is not redundant — sixth guard (Parameters.Length != 0)
    public Task ToCharArrayWithArgumentsNoDiagnosticAsync() => VerifyAsync("""
        public class Test
        {
            public void Run()
            {
                string s = "test";

                foreach (char c in s.ToCharArray(0, 2))
                    System.Console.WriteLine(c);
            }
        }
        """);
}
