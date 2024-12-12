namespace Creedengo.Tests;

[TestClass]
public sealed class VariableCanBeMadeConstantTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<VariableCanBeMadeConstant, VariableCanBeMadeConstantFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

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
}
