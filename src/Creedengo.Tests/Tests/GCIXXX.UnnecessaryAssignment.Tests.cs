namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UnnecessaryAssignmentTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UnecessaryAssignment>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task TestSimpleIfStatement() => VerifyAsync("""
        class C
        {
            int M()
            {
                bool f = false;
                int x = 1; // x
                if (f)
                {
                    x = 2;
                }
                
                return x;
            }
        }
        """);


    [TestMethod]
    public Task TestIfStatement() => VerifyAsync("""
        class C
        {
            int M()
            {
                bool f = false;
                int x = 1; // x
                [|if (f)
                {
                    x = 2;
                }
                else if (f)
                {
                    x = 3;
                }|]

                return x;
            }
        }
        """);

    [TestMethod]
    public Task TestIfStatementNoReturn() => VerifyAsync("""
        class C
        {
            void M()
            {
                bool f = false;
                int x = 1; // x
                if (f)
                {
                    x = 2;
                }
                else if (f)
                {
                    x = 3;
                }

                x = 4;
            }
        }
        """);


    [TestMethod]
    public Task TestIfStatementThrow() => VerifyAsync("""
        using System;

        class C
        {
            int M()
            {
                bool f = false;

                int x = 1;
                [|if (f)
                {
                    x = 2;
                }
                else if (f)
                {
                    x = 3;
                }
                else
                {
                    throw new Exception();
                }|]

                return x;
            }
        }
        """);

    [TestMethod]
    public Task TestSwitchStatement() => VerifyAsync("""
        class C
        {
            int M()
            {
                string s = null;
                int x = 1; // x
                [|switch (s)
                {
                    case "a":
                        {
                            x = 2;
                            break;
                        }
                    case "b":
                        x = 3;
                        break;
                }|]

                return x;
            }
        }
        """);

    [TestMethod]
    public Task TestSwitchStatementThrow() => VerifyAsync("""
        using System;

        class C
        {
            int M()
            {
                string s = null;

                int x = 1;
                [|switch (s)
                {
                    case "a":
                        {
                            x = 2;
                            break;
                        }
                    case "b":
                        x = 3;
                        break;
                    default:
                        throw new Exception();
                }|]

                return x;
            }
        }
        """);

    [TestMethod]
    public Task TestNoDiagnosticForPolymorphicIf() => VerifyAsync("""
        class A {}
        class B {}
        class C
        {
            void M()
            {
                var fun = (bool flag) =>
                {
                    object x;
                    if (flag)
                    {
                        x = new A();
                    }
                    else
                    {
                        x = new B();
                    }

                    return x;
                };
            }
        }
        """);

    [TestMethod]
    public Task TestNoDiagnosticForPolymorphicSwitch() => VerifyAsync("""
        class A {}
        class B {}
        class C
        {
            void M()
            {
                var fun = (object o) =>
                {
                    object x;
                    switch(o)
                    {
                        case int:
                            x = new A();
                            break;
                        default:
                            x = new B();
                            break;
                    }

                    return x;
                };
            }
        }
        """);
}
