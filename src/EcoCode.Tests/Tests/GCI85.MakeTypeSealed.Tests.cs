namespace EcoCode.Tests;

[TestClass]
public sealed class MakeTypeSealedTests
{
    private static readonly AnalyzerDlg AnalyzeAsync = TestRunner.VerifyAsync<MakeTypeSealed>;

    [TestMethod]
    public Task EmptyCodeAsync() => AnalyzeAsync("");

    [TestMethod]
    public Task SealableClassesAsync() => AnalyzeAsync("""
        public class [|TestA|];
        internal class [|TestB|];
        public static class Test0
        {
            public class [|Test01|];
            internal class [|Test02|];
            private class [|Test03|];
        }
        internal static class Test1
        {
            public class [|Test11|];
            internal class [|Test12|];
            private class [|Test13|];
        }
        """);

    [TestMethod]
    public Task SealableRecordsAsync() => AnalyzeAsync("""
        public record [|TestA|];
        internal record [|TestB|];
        public static class Test0
        {
            public record [|Test01|];
            internal record [|Test02|];
            private record [|Test03|];
        }
        internal static class Test1
        {
            public record [|Test11|];
            internal record [|Test12|];
            private record [|Test13|];
        }
        """);

    [TestMethod]
    public Task NonSealableStructsAsync() => AnalyzeAsync("""
        public struct TestA;
        internal struct TestB;
        public static class Test0
        {
            public struct Test01;
            internal struct Test02;
            private struct Test03;
        }
        internal static class Test1
        {
            public struct Test11;
            internal struct Test12;
            private struct Test13;
        }
        """);

    [TestMethod]
    public Task StaticClassesAsync() => AnalyzeAsync("""
        public static class Test0
        {
            public static class Test01;
            internal static class Test02;
            private static class Test03;
        }
        internal static class Test1
        {
            public static class Test11;
            internal static class Test12;
            private static class Test13;
        }
        """);

    [TestMethod]
    public Task AbstractClassesAsync() => AnalyzeAsync("""
        public abstract class TestA;
        internal abstract class TestB;
        public static class Test0
        {
            public abstract class Test01;
            internal abstract class Test02;
            private abstract class Test03;
        }
        internal static class Test1
        {
            public abstract class Test11;
            internal abstract class Test12;
            private abstract class Test13;
        }
        """);

    [TestMethod]
    public Task SealedClassesAsync() => AnalyzeAsync("""
        public sealed class TestA;
        internal sealed class TestB;
        public static class Test0
        {
            public sealed class Test01;
            internal sealed class Test02;
            private sealed class Test03;
        }
        internal static class Test1
        {
            public sealed class Test11;
            internal sealed class Test12;
            private sealed class Test13;
        }
        """);

    [TestMethod]
    public Task SealableClassesWithInterfaceAsync() => AnalyzeAsync("""
        public interface ITest { void Method(); }
        public class [|TestA|] : ITest { public void Method() { } }
        internal class [|TestB|] : ITest { public void Method() { } }
        public static class Test0
        {
            public class [|Test01|] : ITest { public void Method() { } }
            internal class [|Test02|] : ITest { public void Method() { } }
            private class [|Test03|] : ITest { public void Method() { } }
        }
        internal static class Test1
        {
            public class [|Test11|] : ITest { public void Method() { } }
            internal class [|Test12|] : ITest { public void Method() { } }
            private class [|Test13|] : ITest { public void Method() { } }
        }
        """);

    [TestMethod]
    public Task SealableClassesWithOverridableAsync() => AnalyzeAsync("""
        public class TestA1 { public virtual void Method() { } };
        public class [|TestA2|] { internal virtual void Method() { } };
        public class TestA3 { protected virtual void Method() { } };
        public class [|TestA4|] { protected internal virtual void Method() { } };
        public class [|TestA5|] { private protected virtual void Method() { } };

        internal class [|TestB1|] { public virtual void Method() { } };
        internal class [|TestB2|] { internal virtual void Method() { } };
        internal class [|TestB3|] { protected virtual void Method() { } };
        internal class [|TestB4|] { protected internal virtual void Method() { } };
        internal class [|TestB5|] { private protected virtual void Method() { } };

        public static class Test0
        {
            public class TestA1 { public virtual void Method() { } };
            public class [|TestA2|] { internal virtual void Method() { } };
            public class TestA3 { protected virtual void Method() { } };
            public class [|TestA4|] { protected internal virtual void Method() { } };
            public class [|TestA5|] { private protected virtual void Method() { } };
        
            internal class [|TestB1|] { public virtual void Method() { } };
            internal class [|TestB2|] { internal virtual void Method() { } };
            internal class [|TestB3|] { protected virtual void Method() { } };
            internal class [|TestB4|] { protected internal virtual void Method() { } };
            internal class [|TestB5|] { private protected virtual void Method() { } };

            private class [|TestC1|] { public virtual void Method() { } };
            private class [|TestC2|] { internal virtual void Method() { } };
            private class [|TestC3|] { protected virtual void Method() { } };
            private class [|TestC4|] { protected internal virtual void Method() { } };
            private class [|TestC5|] { private protected virtual void Method() { } };
        }

        internal static class Test1
        {
            public class [|TestA1|] { public virtual void Method() { } };
            public class [|TestA2|] { internal virtual void Method() { } };
            public class [|TestA3|] { protected virtual void Method() { } };
            public class [|TestA4|] { protected internal virtual void Method() { } };
            public class [|TestA5|] { private protected virtual void Method() { } };
        
            internal class [|TestB1|] { public virtual void Method() { } };
            internal class [|TestB2|] { internal virtual void Method() { } };
            internal class [|TestB3|] { protected virtual void Method() { } };
            internal class [|TestB4|] { protected internal virtual void Method() { } };
            internal class [|TestB5|] { private protected virtual void Method() { } };
        
            private class [|TestC1|] { public virtual void Method() { } };
            private class [|TestC2|] { internal virtual void Method() { } };
            private class [|TestC3|] { protected virtual void Method() { } };
            private class [|TestC4|] { protected internal virtual void Method() { } };
            private class [|TestC5|] { private protected virtual void Method() { } };
        }
        """);

    [TestMethod]
    public Task InheritanceAsync() => AnalyzeAsync("""
        public abstract class Test2 { public virtual void Overridable() { } }
        public class Test3 : Test2;
        public sealed class Test4 : Test3;
        public class Test5 : Test3;
        public class Test6 : Test3 { public override void Overridable() { } }
        public class [|Test7|] : Test3 { public sealed override void Overridable() { } }
        public class Test8 : Test3 { public sealed override void Overridable() { } }
        public class [|Test9|] : Test8;
        """);

    [TestMethod]
    public Task PartialAsync() => AnalyzeAsync("""
        public partial class [|Test1|];
        partial class Test1 { public void Method() { } }

        partial class [|Test2|] { public void Method() { } }
        public partial class Test2;

        public partial class Test3;
        partial class Test3 { public virtual void Method() { } }

        public partial class Test4;
        sealed partial class Test4 { public void Method() { } }
        """);

    [TestMethod]
    public Task Partial2Async() => AnalyzeAsync("""
        public partial class [|Test1|];
        partial class Test1(int Value) { public int Method() => Value; }

        partial record [|Test2|]
        {
            private readonly int Value;
            public Test2(int value) => Value = value;
            public int Method() => Value;
        }
        public partial record Test2;

        public partial class Test3;
        partial class Test3 { public virtual void Method() { } }

        public partial class Test4;
        sealed partial class Test4(int Value) { public int Method() => Value; }
        """);
}
