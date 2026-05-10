using System.Buffers;

namespace Creedengo.Tool.Services;

static partial class ReportService
{
    private static class Csv
    {
        private const string Header = "Directory;File;Location;Severity;Code;Message";

        private static readonly SearchValues<char> CharsRequiringQuotes = SearchValues.Create(";\"\r\n");

        public static async Task WriteToStreamAsync(StreamWriter writer, List<DiagnosticInfo> diagnostics, CancellationToken cancellationToken = default)
        {
            await writer.WriteLineAsync(Header.AsMemory(), cancellationToken).ConfigureAwait(false);

            foreach (var diag in diagnostics)
                await writer.WriteLineAsync($"{Escape(diag.Directory)};{Escape(diag.File)};{Escape(diag.Location)};{Escape(diag.Severity)};{Escape(diag.Code)};{Escape(diag.Message)}".AsMemory(), cancellationToken).ConfigureAwait(false);
        }

        // RFC 4180 escaping with ';' as the separator: quote the field if it contains ';', '"', CR, or LF; double any embedded '"'.
        private static string Escape(string field) =>
            field.AsSpan().IndexOfAny(CharsRequiringQuotes) < 0
                ? field
                : $"\"{field.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
