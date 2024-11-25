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
}
