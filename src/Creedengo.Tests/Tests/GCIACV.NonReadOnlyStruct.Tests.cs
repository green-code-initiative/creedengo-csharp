namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class NonReadOnlyStructTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<NonReadOnlyStruct, NonReadOnlyStructFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task NoWarningOnClass() => VerifyAsync("""
        public class Test
        {
            public void Method(in Test test)
            {
            }
        }
        """);

    [TestMethod]
    public Task NoWarningOnClassReference() => VerifyAsync("""
        public class Test
        {
            public void Method(in string test)
            {
            }
        }
        """);

    [TestMethod]
    public Task NoWarningOnReadOnlyStruct() => VerifyAsync("""
        public readonly struct ReadOnlyTest
        {
            public void Method(in ReadOnlyTest test)
            {
            }
        }
        """);

    [TestMethod]
    public Task NoWarningOnParameterWithoutInModifier() => VerifyAsync("""
        public struct Test
        {
            public void Method(Test test)
            {
            }
        }
        """);

    [TestMethod]
    public Task WarningOnNonReadOnlyStructWithInModifier() => VerifyAsync("""
        public struct Test
        {
            public void Method(in [|Test|] test)
            {
            }
        }
        """, """
        public struct Test
        {
            public void Method(Test test)
            {
            }
        }
        """);

    [TestMethod]
    public Task WarningOnMultipleParametersWithInModifier() => VerifyAsync("""
        public struct Test
        {
            public void Method(in [|Test|] test1, int value, in [|Test|] test2)
            {
            }
        }
        """, """
        public struct Test
        {
            public void Method(Test test1, int value, Test test2)
            {
            }
        }
        """);

    [TestMethod]
    public Task WarningOnNestedNonReadOnlyStructs() => VerifyAsync("""
        public struct OuterStruct
        {
            public struct InnerStruct
            {
            }

            public void Method(in [|InnerStruct|] inner)
            {
            }
        }
        """, """
        public struct OuterStruct
        {
            public struct InnerStruct
            {
            }

            public void Method(InnerStruct inner)
            {
            }
        }
        """);

    [TestMethod]
    public Task WarningOnStructWithComplexMethodBody() => VerifyAsync("""
        public struct Test
        {
            private int _value;

            public void Method(in [|Test|] test)
            {
                var value = test._value;
                System.Console.WriteLine(value);
                System.Console.WriteLine("Processing completed");
            }
        }
        """, """
        public struct Test
        {
            private int _value;

            public void Method(Test test)
            {
                var value = test._value;
                System.Console.WriteLine(value);
                System.Console.WriteLine("Processing completed");
            }
        }
        """);

    [TestMethod]
    public Task WarningOnStructInExtensionMethod() => VerifyAsync("""
        public static class Extensions
        {
            public static void Extend(this in [|TestStruct|] test)
            {
            }
        }

        public struct TestStruct
        {
        }
        """, """
        public static class Extensions
        {
            public static void Extend(this TestStruct test)
            {
            }
        }

        public struct TestStruct
        {
        }
        """);
        
    [TestMethod]
    public Task NoWarningOnRefReadOnlyWithReadOnlyStruct() => VerifyAsync("""
        public readonly struct ReadOnlyTest
        {
            public void Method(ref readonly ReadOnlyTest test)
            {
            }
        }
        """);
        
    [TestMethod]
    public Task WarningOnRefReadOnlyWithNonReadOnlyStruct() => VerifyAsync("""
        public struct Test
        {
            public void Method(ref readonly [|Test|] test)
            {
            }
        }
        """, """
        public struct Test
        {
            public void Method(ref Test test)
            {
            }
        }
        """);
          [TestMethod]
    public Task WarningOnRefReadOnlyWithNonReadOnlyStructRemoveReadOnly() => VerifyAsync("""
        public struct Test
        {
            public void Method(ref readonly [|Test|] test)
            {
            }
        }
        """, """
        public struct Test
        {
            public void Method(ref Test test)
            {
            }
        }
        """);
          [TestMethod]
    public Task WarningOnRefReadOnlyWithNonReadOnlyStructRemoveBoth() => VerifyAsync("""
        public struct Test
        {
            public void Method(ref readonly [|Test|] test)
            {
            }
        }
        """, """
        public struct Test
        {
            public void Method(ref Test test)
            {
            }
        }
        """);
          [TestMethod]
    public Task WarningOnMixedModifiersWithRefReadOnlyAndIn() => VerifyAsync("""
        public struct Test
        {
            public void Method(ref readonly [|Test|] test1, int value, in [|Test|] test2)
            {
            }
        }
        """);
}
