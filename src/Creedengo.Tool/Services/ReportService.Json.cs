using System.Text.Json;

namespace Creedengo.Tool.Services;

static partial class ReportService
{
    private static class Json
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public static async Task WriteToStreamAsync(StreamWriter writer, List<DiagnosticInfo> diagnostics, CancellationToken cancellationToken = default)
        {
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            await JsonSerializer.SerializeAsync(writer.BaseStream, diagnostics, Options, cancellationToken).ConfigureAwait(false);
        }
    }
}
