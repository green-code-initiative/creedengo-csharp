namespace Creedengo.Tool.Services;

static partial class ReportService
{
    private static class Csv
    {
        private const string Header = "Directory;File;Location;Severity;Code;Message";

        public static async Task WriteToStreamAsync(StreamWriter writer, List<DiagnosticInfo> diagnostics)
        {
            await writer.WriteLineAsync(Header).ConfigureAwait(false);

            foreach (var diag in diagnostics)
                await writer.WriteLineAsync($"{diag.Directory};{diag.File};{diag.Location};{diag.Severity};{diag.Code};{diag.Message}").ConfigureAwait(false);
        }
    }
}
