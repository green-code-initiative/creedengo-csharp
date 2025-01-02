using Creedengo.Tool.Services;
using Microsoft.CodeAnalysis.MSBuild;

namespace Creedengo.Tool.Commands;

internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    public override ValidationResult Validate(CommandContext context, AnalyzeSettings settings)
    {
        if (!File.Exists(settings.Source))
            return ValidationResult.Error($"The source file {settings.Source} does not exist.");

        if (Path.GetDirectoryName(settings.Output) is { } outputDir)
        {
            try
            {
                _ = Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"The output directory {outputDir} cannot be opened or created: {ex.Message}");
            }
        }

        return base.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings)
    {
        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Program.WriteLine(e.Diagnostic.Message, "red");

        var analysisService = await AnalysisService.CreateAsync(settings.SeverityLevel).ConfigureAwait(false);

        var diagnostics = new List<DiagnosticInfo>();

        if (settings.SourceType is SourceType.Solution)
        {
            Solution solution;
            try
            {
                solution = await workspace.OpenSolutionAsync(settings.Source).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Program.WriteLine($"Cannot load the provided solution: {ex.Message}", "red");
                Program.WriteLine(ex.StackTrace!);
                return 1;
            }

            foreach (var project in solution.Projects)
                await analysisService.AnalyzeProjectAsync(project, diagnostics).ConfigureAwait(false);
        }
        else // options.SourceType is SourceType.Project
        {
            Project project;
            try
            {
                project = await workspace.OpenProjectAsync(settings.Source).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Program.WriteLine($"Cannot load the provided project: {ex.Message}", "red");
                Program.WriteLine(ex.StackTrace!);
                return 1;
            }

            await analysisService.AnalyzeProjectAsync(project, diagnostics).ConfigureAwait(false);
        }

        await ReportService.GenerateReportAsync(diagnostics, settings.Output, settings.OutputType).ConfigureAwait(false);

        return 0;
    }
}
