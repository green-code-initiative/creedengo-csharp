namespace Creedengo.Tool.Services;

internal static partial class ReportService
{
    public static async Task GenerateReportAsync(List<DiagnosticInfo> diagnostics, string outputPath, OutputType outputType)
    {
        var writer = new StreamWriter(outputPath, append: false);
        await using (writer.ConfigureAwait(false)) // In two steps to avoid CA2008 Consider calling ConfigureAwait on the awaited task
        {
            switch (outputType)
            {
                case OutputType.Html: await Html.WriteToStreamAsync(writer, diagnostics).ConfigureAwait(false); break;
                case OutputType.Json: await Json.WriteToStreamAsync(writer, diagnostics).ConfigureAwait(false); break;
                case OutputType.Csv: await Csv.WriteToStreamAsync(writer, diagnostics).ConfigureAwait(false); break;
                default: throw new NotSupportedException();
            }
        }
    }
}
