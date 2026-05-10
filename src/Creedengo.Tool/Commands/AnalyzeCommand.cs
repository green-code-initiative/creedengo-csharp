using Creedengo.Tool.Services;
using Microsoft.CodeAnalysis.MSBuild;

namespace Creedengo.Tool.Commands;

internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    protected override ValidationResult Validate(CommandContext context, AnalyzeSettings settings)
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

    protected override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken cancellationToken)
    {
        using var workspace = MSBuildWorkspace.Create();
#pragma warning disable CS0618 // RegisterWorkspaceFailedHandler requires Roslyn ≥ 4.13
        workspace.WorkspaceFailed += (sender, e) => Program.WriteLine(e.Diagnostic.Message, "red");
#pragma warning restore CS0618

        var analysisService = await AnalysisService.CreateAsync(settings.SeverityLevel).ConfigureAwait(false);

        var diagnostics = new List<DiagnosticInfo>();

        if (settings.SourceType is SourceType.Solution)
        {
            Solution solution;
            try
            {
                solution = await workspace.OpenSolutionAsync(settings.Source, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Program.WriteLine($"Cannot load the provided solution: {ex.Message}", "red");
                Program.WriteLine(ex.StackTrace!);
                return 1;
            }

            foreach (var project in solution.Projects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await analysisService.AnalyzeProjectAsync(project, diagnostics, cancellationToken).ConfigureAwait(false);
            }
        }
        else // options.SourceType is SourceType.Project
        {
            Project project;
            try
            {
                project = await workspace.OpenProjectAsync(settings.Source, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Program.WriteLine($"Cannot load the provided project: {ex.Message}", "red");
                Program.WriteLine(ex.StackTrace!);
                return 1;
            }

            await analysisService.AnalyzeProjectAsync(project, diagnostics, cancellationToken).ConfigureAwait(false);
        }

        await ReportService.GenerateReportAsync(diagnostics, settings.Output, settings.OutputType, cancellationToken).ConfigureAwait(false);

        return 0;
    }
}
