namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class SpecifyStructLayoutTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<SpecifyStructLayout, SpecifyStructLayoutFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task EmptyStructWithNoLayoutAsync() => VerifyAsync(
        "public record struct TestStruct;");

    [TestMethod]
    public Task ValuePropWithNoLayoutAsync() => VerifyAsync(
        "public record struct TestStruct(int A);");

    [TestMethod]
    public Task ReferencePropWithNoLayoutAsync() => VerifyAsync(
        "public record struct TestStruct(string A);");

    [TestMethod]
    // For some reason this test doesn't pass with the 'record' syntax on GitHub, but it passes locally..
    public Task ValuePropsWithNoLayoutAsync() => VerifyAsync("""
        public struct [|TestStruct|]
        {
            public int A { get; set; }
            public double B { get; set; }
        };
        """, """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A { get; set; }
            public double B { get; set; }
        };
        """);

    [TestMethod]
    public Task ValuePropsWithLayoutAsync() => VerifyAsync("""
        using System.Runtime.InteropServices;
        
        [StructLayout(LayoutKind.Auto)]
        public record struct TestStruct(int A, double B);
        """);

    [TestMethod]
    public Task ValueAndReferencePropsWithNoLayoutAsync() => VerifyAsync(
        "public record struct TestStruct(int A, string B);");

    [TestMethod]
    // For some reason this test doesn't pass with the 'record' syntax on GitHub, but it passes locally..
    public Task AdditionalValuePropsWithNoLayout1Async() => VerifyAsync("""
        public struct [|TestStruct|]
        {
            public int A { get; set; }
            public double B { get; set; }
            public int C { get; set; }
        };
        """, """
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public struct TestStruct
        {
            public int A { get; set; }
            public double B { get; set; }
            public int C { get; set; }
        };
        """);

    [TestMethod]
    public Task AdditionalValueFieldsWithNoLayout2Async() => VerifyAsync("""
        using System;

        public record struct [|TestStruct|](bool A, int B, char C, short D, ulong E, DateTime F);
        """, """
        using System;
        using System.Runtime.InteropServices;

        [StructLayout(LayoutKind.Auto)]
        public record struct [|TestStruct|](bool A, int B, char C, short D, ulong E, DateTime F);
        """);
}
