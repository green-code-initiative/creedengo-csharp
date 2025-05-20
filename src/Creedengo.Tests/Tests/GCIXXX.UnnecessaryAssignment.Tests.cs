namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UnnecessaryAssignmentTests
{
    //private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UnecessaryAssignment, UnnecessaryAssignmentFixer>;
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UnecessaryAssignment>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    //[TestMethod]
    //public Task NominalCase() => VerifyAsync("""
    //    class TestClass
    //    {
    //        int TestMethod()
    //        {
    //            if (condition)
    //            {
    //                x = 1;
    //            }
    //            else
    //            {
    //                x = 2;
    //            }

    //            return x;
    //        }
    //    }
    //    """, """
    //    class TestClass
    //    {
    //        int TestMethod()
    //        {
    //            if (condition)
    //            {
    //                return 1;
    //            }
    //            else
    //            {
    //                return 2;
    //            }
    //        }
    //    }
    //    """);

    [TestMethod]
    public Task NominalCase() => VerifyAsync("""
        class TestClass
        {
            int TestMethod(bool condition)
            {
                int x;
                if (condition)
                {
                    [|x = 1|];
                }
                else
                {
                    [|x = 2|];
                }

                return x;
            }
        }
        """);


    [TestMethod]
    public Task NominalCase2() => VerifyAsync("""
        class TestClass
        {
            int TestMethod(bool condition)
            {
                int x;
                if (condition)
                {
                    [|x = 1|];
                }
                else
                {
                    [|x = 2|];
                }

                [|x += 3|];
                return x;
            }
        }
        """);

    [TestMethod]
    public Task NominalCase3() => VerifyAsync("""
        class TestClass
        {
            int TestMethod(bool condition)
            {
                int x;
                
                [|x = 1|];
                
                [|x = 2|];
                [|x = 3|];
                [|x = 4|];
        
                return x;
            }
        }
        """);

    [TestMethod]
    public Task NominalCase4() => VerifyAsync("""
        class TestClass
        {
            (int, int) TestMethod(bool condition)
            {
                var x = (10, 20);
                
                [|x = (15, 30)|];
                
                [|x = (20, 21)|];
                
                return x;
            }
        }
        """);
}
