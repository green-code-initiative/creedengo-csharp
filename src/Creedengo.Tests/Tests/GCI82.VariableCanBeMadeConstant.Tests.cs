namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class VariableCanBeMadeConstantTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<VariableCanBeMadeConstant, VariableCanBeMadeConstantFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontWarnOnConstAsync() => VerifyAsync("""
        public class TestClass
        {
            private const int x = 1;
            private const string s = "Bar";
        }
        """);

    [TestMethod]
    public Task WarnOnStaticReadonlyIntAsync() => VerifyAsync("""
        public class TestClass
        {
            [|private static readonly int x = 1;|]
        }
        """, """
        public class TestClass
        {
            private const int x = 1;
        }
        """);

    [TestMethod]
    public Task WarnOnStaticReadonlyStringAsync() => VerifyAsync("""
        public class TestClass
        {
            [|private static readonly string s = "Bar";|]
        }
        """, """
        public class TestClass
        {
            private const string s = "Bar";
        }
        """);

    [TestMethod]
    public Task DontWarnOnStaticReadonlyNonConstTypeAsync() => VerifyAsync("""
        public class TestClass
        {
            private static readonly object o = new object();
        }
        """);

    [TestMethod]
    public Task DontWarnOnStaticReadonlyNonConstantValueAsync() => VerifyAsync("""
        public class TestClass
        {
            private static readonly int x = System.Environment.TickCount;
        }
        """);

    [TestMethod]
    public Task WarnOnMultipleStaticReadonlyFieldsAsync() => VerifyAsync("""
        public class TestClass
        {
            [|private static readonly int a = 1;|]
            [|private static readonly string b = "foo";|]
        }
        """, """
        public class TestClass
        {
            private const int a = 1;
            private const string b = "foo";
        }
        """);

    [TestMethod]
    public Task DontWarnOnStaticReadonlyWithoutInitializerAsync() => VerifyAsync("""
        public class TestClass
        {
            private static readonly int x;
        }
        """);

    [TestMethod]
    public Task VariableCanBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                [|int i = 0;|]
                Console.WriteLine(i);
            }
        }
        """, """
        using System;
        public class Program
        {
            public static void Main()
            {
                const int i = 0;
                Console.WriteLine(i);
            }
        }
        """);

    [TestMethod]
    public Task VariableIsReassignedAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                int i = 0;
                Console.WriteLine(i++);
            }
        }
        """);

    [TestMethod]
    public Task VariableIsAlreadyConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                const int i = 0;
                Console.WriteLine(i);
            }
        }
        """);

    [TestMethod]
    public Task VariableHasNoInitializerAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                int i;
                i = 0;
                Console.WriteLine(i);
            }
        }
        """);

    [TestMethod]
    public Task VariableCannotBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                int i = DateTime.Now.DayOfYear;
                Console.WriteLine(i);
            }
        }
        """);

    [TestMethod]
    public Task VariableWithMultipleInitializersCannotBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                int i = 0, j = DateTime.Now.DayOfYear;
                Console.WriteLine(i);
                Console.WriteLine(j);
            }
        }
        """);

    [TestMethod]
    public Task VariableWithMultipleInitializersCanBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                [|int i = 0, j = 0;|]
                Console.WriteLine(i);
                Console.WriteLine(j);
            }
        }
        """, """
        using System;
        public class Program
        {
            public static void Main()
            {
                const int i = 0, j = 0;
                Console.WriteLine(i);
                Console.WriteLine(j);
            }
        }
        """);

    [TestMethod]
    public Task VariableInitializerIsInvalidAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                int x = {|CS0029:"abc"|};
            }
        }
        """);

    [TestMethod]
    public Task StringObjectCannotBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                object s = "abc";
            }
        }
        """);

    [TestMethod]
    public Task StringCanBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                [|string s = "abc";|]
            }
        }
        """, """
        using System;
        public class Program
        {
            public static void Main()
            {
                const string s = "abc";
            }
        }
        """);

    [TestMethod]
    public Task VarIntCanBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                [|var item = 4;|]
            }
        }
        """, """
        using System;
        public class Program
        {
            public static void Main()
            {
                const int item = 4;
            }
        }
        """);

    [TestMethod]
    public Task VarStringCanBeConstAsync() => VerifyAsync("""
        using System;
        public class Program
        {
            public static void Main()
            {
                [|var item = "abc";|]
            }
        }
        """, """
        using System;
        public class Program
        {
            public static void Main()
            {
                const string item = "abc";
            }
        }
        """);

    [TestMethod]
    public Task WarnOnNullReferenceTypeInitializerAsync() => VerifyAsync("""
        public static class Test
        {
            public static void Main()
            {
                [|string item = null;|]
                System.Console.WriteLine(item);
            }
        }
        """, """
        public static class Test
        {
            public static void Main()
            {
                const string item = null;
                System.Console.WriteLine(item);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnNullableValueTypeInitializerAsync() => VerifyAsync("""
        public static class Test
        {
            public static void Main()
            {
                int? item = null; // 'const int?' is not allowed (CS0283), so no const suggestion.
                System.Console.WriteLine(item);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnUserStructInitializerAsync() => VerifyAsync("""
        public static class Test
        {
            public struct Point { public int X; }
            public static void Main()
            {
                Point item = default; // User-defined structs cannot be const.
                System.Console.WriteLine(item);
            }
        }
        """);
}
