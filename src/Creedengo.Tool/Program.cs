using Microsoft.Build.Locator;

namespace Creedengo.Tool;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        _ = MSBuildLocator.RegisterDefaults();

        var app = new CommandApp();
        app.Configure(config => config.AddCommand<AnalyzeCommand>("analyze"));
        int errorCode = await app.RunAsync(args).ConfigureAwait(false);

        if (!Console.IsOutputRedirected) // Interactive mode
        {
            WriteLine("Press a key to exit..");
            _ = Console.ReadKey();
        }

        return errorCode;
    }

    public static void WriteLine(string line) => AnsiConsole.WriteLine(line);

    public static void WriteLine(string line, string color) => AnsiConsole.MarkupLine($"[{color}]{line}[/]");
}
