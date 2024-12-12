using Microsoft.CodeAnalysis.Text;
using System.Threading;

namespace Creedengo.Tool.Common;

internal sealed class AdditionalFile(string path, string text) : AdditionalText
{
    public const string GlobalConfigFile = "Creedengo.globalconfig";

    public override string Path { get; } = path;

    public string Text { get; } = text;

    public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(Text);

    public static async Task<AdditionalFile> LoadGlobalConfigAsync()
    {
        string editorConfigPath = System.IO.Path.Combine(AppContext.BaseDirectory, GlobalConfigFile);
        if (!File.Exists(editorConfigPath))
            throw new FileNotFoundException($"Editor config file not found at {editorConfigPath}");

        string fileContent = await File.ReadAllTextAsync(editorConfigPath).ConfigureAwait(false);
        return new AdditionalFile(GlobalConfigFile, fileContent.Replace("is_global = true", string.Empty, StringComparison.OrdinalIgnoreCase));
    }
}
