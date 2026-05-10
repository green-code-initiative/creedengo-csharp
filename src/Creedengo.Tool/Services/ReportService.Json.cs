using System.Text.Json;

namespace Creedengo.Tool.Services;

static partial class ReportService
{
    private static class Json
    {
        public static async Task WriteToStreamAsync(StreamWriter writer, List<DiagnosticInfo> diagnostics, CancellationToken cancellationToken = default)
        {
            if (diagnostics.Count == 0)
            {
                await writer.WriteLineAsync("[]".AsMemory(), cancellationToken).ConfigureAwait(false);
                return;
            }

            await writer.WriteLineAsync("[".AsMemory(), cancellationToken).ConfigureAwait(false);

            for (int i = 0; i < diagnostics.Count - 1; i++)
                await writer.WriteLineAsync((JsonSerializer.Serialize(diagnostics[i]) + ',').AsMemory(), cancellationToken).ConfigureAwait(false);

            await writer.WriteLineAsync(JsonSerializer.Serialize(diagnostics[^1]).AsMemory(), cancellationToken).ConfigureAwait(false);

            await writer.WriteLineAsync("]".AsMemory(), cancellationToken).ConfigureAwait(false);
        }
    }
}
