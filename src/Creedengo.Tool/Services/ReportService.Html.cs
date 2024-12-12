using System.Globalization;
using System.Text;

namespace Creedengo.Tool.Services;

static partial class ReportService
{
    private static class Html
    {
        private const string Header = """
        <html>
        <head>
            <style>
                table {width: 100%; border-collapse: collapse;}
                th, td {border: 1px solid black; padding: 8px; text-align: left;}
                th {background-color: #f2f2f2;}
            </style>
        </head>
        <body>
            <h1>Creedengo analyzer report</h1>
            <table>
                <tr>
                    <th>Directory</th>
                    <th>File</th>
                    <th>Location</th>
                    <th>Severity</th>
                    <th>Code</th>
                    <th>Message</th>
                </tr>
        """;

        private const string Footer = """
            </table>
        </body>
        </html>
        """;

        private static readonly CompositeFormat Row = CompositeFormat.Parse("""
              <tr>
                 <td>{0}</td>
                 <td>{1}</td>
                 <td>{2}</td>
                 <td>{3}</td>
                 <td>{4}</td>
                 <td>{5}</td>
              </tr>
        """);

        public static async Task WriteToStreamAsync(StreamWriter writer, List<DiagnosticInfo> diagnostics)
        {
            await writer.WriteLineAsync(Header).ConfigureAwait(false);

            foreach (var diag in diagnostics)
                await writer.WriteLineAsync(string.Format(CultureInfo.InvariantCulture, Row, diag.Directory, diag.File, diag.Location, diag.Severity, diag.Code, diag.Message)).ConfigureAwait(false);

            await writer.WriteLineAsync(Footer).ConfigureAwait(false);
        }
    }
}
