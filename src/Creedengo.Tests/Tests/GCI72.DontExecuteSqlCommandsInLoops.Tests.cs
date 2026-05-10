namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class DontExecuteSqlCommandsInLoopsTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<DontExecuteSqlCommandsInLoops>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontExecuteSqlCommandsInLoopsAsync() => VerifyAsync("""
        using System.Data;
        public class Test
        {
            public void Run(int p)
            {
                var command = default(IDbCommand)!;
                _ = command.ExecuteNonQuery();
                _ = command.ExecuteScalar();
                _ = command.ExecuteReader();
                _ = command.ExecuteReader(CommandBehavior.Default);

                for (int i = 0; i < 10; i++)
                {
                    _ = [|command.ExecuteNonQuery()|];
                    _ = [|command.ExecuteScalar()|];
                    _ = [|command.ExecuteReader()|];
                    _ = [|command.ExecuteReader(CommandBehavior.Default)|];
                }
            }
        }
        """);

    [TestMethod]
    public Task WarnInForeachLoopAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Data;
        public class Test
        {
            public void Run(IEnumerable<int> items)
            {
                var command = default(IDbCommand)!;
                foreach (var item in items)
                {
                    _ = [|command.ExecuteNonQuery()|];
                }
            }
        }
        """);

    [TestMethod]
    public Task WarnInWhileLoopAsync() => VerifyAsync("""
        using System.Data;
        public class Test
        {
            public void Run()
            {
                var command = default(IDbCommand)!;
                int i = 0;
                while (i < 10)
                {
                    _ = [|command.ExecuteScalar()|];
                    i++;
                }
            }
        }
        """);

    [TestMethod]
    public Task WarnInDoWhileLoopAsync() => VerifyAsync("""
        using System.Data;
        public class Test
        {
            public void Run()
            {
                var command = default(IDbCommand)!;
                int i = 0;
                do
                {
                    _ = [|command.ExecuteReader()|];
                    i++;
                } while (i < 10);
            }
        }
        """);
}
