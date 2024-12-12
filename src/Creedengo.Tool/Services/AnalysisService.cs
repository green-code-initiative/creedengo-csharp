using Creedengo.Analyzers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Creedengo.Tool.Services;

internal sealed partial class AnalysisService
{
    private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

    private readonly AnalyzerOptions _analyzerOptions;

    private AnalysisService(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions analyzerOptions)
    {
        _analyzers = analyzers;
        _analyzerOptions = analyzerOptions;
    }

    [GeneratedRegex(@"dotnet_diagnostic\.(.+)\.severity(?:\s*=\s*)(\w+)")]
    private static partial Regex ParseAnalyzersRegex();

    public static async Task<AnalysisService> CreateAsync(DiagnosticSeverity minSeverity)
    {
        var globalConfig = await AdditionalFile.LoadGlobalConfigAsync().ConfigureAwait(false);

        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

        analyzers.AddRange(LoadCreedengoAnalyzers(minSeverity));
        int creedengoAnalyzersCount = analyzers.Count;

        analyzers.AddRange(LoadRoslynAnalyzers(globalConfig.Text, minSeverity));
        Program.WriteLine($"Using {creedengoAnalyzersCount} Creedengo analyzers and {analyzers.Count - creedengoAnalyzersCount} customized Roslyn analyzers");

        return new(analyzers.DrainToImmutable(), new AnalyzerOptions([globalConfig]));
    }

    private static IEnumerable<DiagnosticAnalyzer> LoadCreedengoAnalyzers(DiagnosticSeverity minSeverity) =>
        typeof(DontCallFunctionsInLoopConditions).Assembly.GetTypes()
        .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DiagnosticAnalyzer)))
        .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)
        .Where(analyzer => analyzer.SupportedDiagnostics.Any(diag => diag.DefaultSeverity >= minSeverity));

    private static IEnumerable<DiagnosticAnalyzer> LoadRoslynAnalyzers(string config, DiagnosticSeverity minSeverity)
    {
        var roslynAnalyzerIds = new HashSet<string>();
        foreach (Match match in ParseAnalyzersRegex().Matches(config))
        {
            if (match.Groups.Count == 3 && Enum.TryParse<DiagnosticSeverity>(match.Groups[2].ValueSpan, ignoreCase: true, out var severity) && severity >= minSeverity)
                _ = roslynAnalyzerIds.Add(match.Groups[1].Value);
        }
        if (roslynAnalyzerIds.Count == 0) yield break;

        var roslynAnalyzerTypes = Assembly.Load("Microsoft.CodeAnalysis.NetAnalyzers").GetTypes()
            .Union(Assembly.Load("Microsoft.CodeAnalysis.CSharp.NetAnalyzers").GetTypes())
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DiagnosticAnalyzer)));

        foreach (var roslynAnalyzerType in roslynAnalyzerTypes)
        {
            var roslynAnalyzer = (DiagnosticAnalyzer)Activator.CreateInstance(roslynAnalyzerType)!;
            foreach (var supportedDiagnostic in roslynAnalyzer.SupportedDiagnostics)
            {
                if (roslynAnalyzerIds.Contains(supportedDiagnostic.Id))
                {
                    yield return roslynAnalyzer;
                    break;
                }
            }
        }
    }

    public async Task AnalyzeProjectAsync(Project project, List<DiagnosticInfo> diagnostics)
    {
        Program.WriteLine($"Analyzing project {project.Name}...", "darkorange");

        if (await project.GetCompilationAsync().ConfigureAwait(false) is not { } compilation)
        {
            Program.WriteLine($"Unable to load the project {project.Name} compilation, skipping.", "red");
            return;
        }

        var compilationWithAnalyzers = compilation.WithAnalyzers(_analyzers, _analyzerOptions);
        foreach (var diagnostic in await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false))
            diagnostics.Add(DiagnosticInfo.FromDiagnostic(diagnostic));

        Program.WriteLine($"Analysis complete for project {project.Name}", "green");
    }
}
