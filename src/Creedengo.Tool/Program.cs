using Microsoft.Build.Locator;
using System.Reflection;

namespace Creedengo.Tool;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        _ = MSBuildLocator.RegisterDefaults();

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationVersion(GetDisplayVersion());
            config.AddCommand<AnalyzeCommand>("analyze");
        });
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

    // AssemblyInformationalVersion includes the package version (e.g. "3.0.0" or "3.0.0-beta1") plus a SourceLink "+<gitsha>" suffix.
    // Drop the suffix for the user-facing display; the full string is still readable via reflection on the binary.
    private static string GetDisplayVersion()
    {
        var info = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
        int plus = info.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? info : info.Substring(0, plus);
    }
}
