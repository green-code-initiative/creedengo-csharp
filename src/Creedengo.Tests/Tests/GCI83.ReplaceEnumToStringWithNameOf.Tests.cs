namespace Creedengo.Tests;

[TestClass]
public sealed class ReplaceEnumToStringWithNameOfTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<ReplaceEnumToStringWithNameOf, ReplaceEnumToStringWithNameOfFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task EnumToStringShouldNotBeNameOfAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine(MyEnum.A.ToString());
                Console.WriteLine(MyEnum.B.ToString(""));
                Console.WriteLine(MyEnum.C.ToString(string.Empty));
                Console.WriteLine(MyEnum.D.ToString(format: null));
            }
        }
        """, languageVersion: LanguageVersion.CSharp5);

    [TestMethod]
    public Task EnumToStringShouldBeNameOfAsync1() => VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine([|MyEnum.A.ToString()|]);
                Console.WriteLine([|MyEnum.B.ToString("")|]);
                Console.WriteLine([|MyEnum.C.ToString(string.Empty)|]);
                Console.WriteLine([|MyEnum.D.ToString(format: null)|]);
            }
        }
        """, """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine(nameof(MyEnum.A));
                Console.WriteLine(nameof(MyEnum.B));
                Console.WriteLine(nameof(MyEnum.C));
                Console.WriteLine(nameof(MyEnum.D));
            }
        }
        """);

    [TestMethod]
    public Task EnumToStringShouldBeNameOfAsync2() => VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C }
            public static void Main()
            {
                Console.WriteLine([|MyEnum.A.ToString("G")|]);
                Console.WriteLine([|MyEnum.B.ToString("F")|]);
                Console.WriteLine(MyEnum.C.ToString("N"));
            }
        }
        """, """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C }
            public static void Main()
            {
                Console.WriteLine(nameof(MyEnum.A));
                Console.WriteLine(nameof(MyEnum.B));
                Console.WriteLine(MyEnum.C.ToString("N"));
            }
        }
        """);

    [TestMethod]
    public Task EnumInterpolationShouldBeNameOfAsync() => VerifyAsync("""
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine($"[|{MyEnum.A}|]");
                Console.WriteLine($"[|{MyEnum.B:G}|]");
                Console.WriteLine($"[|{MyEnum.C:F}|]");
                Console.WriteLine($"{MyEnum.D:N}");
            }
        }
        """, """
        using System;
        public static class Program
        {
            private enum MyEnum { A, B, C, D }
            public static void Main()
            {
                Console.WriteLine($"{nameof(MyEnum.A)}");
                Console.WriteLine($"{nameof(MyEnum.B)}");
                Console.WriteLine($"{nameof(MyEnum.C)}");
                Console.WriteLine($"{MyEnum.D:N}");
            }
        }
        """);
}
