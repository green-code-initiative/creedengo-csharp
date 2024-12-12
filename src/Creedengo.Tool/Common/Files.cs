namespace Creedengo.Tool.Common;

internal enum SourceType
{
    Unknown,
    Project,
    Solution
}

internal enum OutputType
{
    Unknown,
    Html,
    Json,
    Csv
}

internal static class Files
{
    private const string Project = ".csproj";

    private static readonly ImmutableArray<string> Solution = [".sln", ".slnf", ".slnx"];

    public static SourceType GetSourceType(string source)
    {
        string? ext = Path.GetExtension(source);
        return string.IsNullOrWhiteSpace(ext) ? SourceType.Unknown
            : string.Equals(ext, Project, StringComparison.OrdinalIgnoreCase) ? SourceType.Project
            : Solution.Contains(ext, StringComparer.OrdinalIgnoreCase) ? SourceType.Solution
            : SourceType.Unknown;
    }

    public static OutputType GetOutputType(string output) =>
        Enum.TryParse<OutputType>(Path.GetExtension(output)?[1..], ignoreCase: true, out var extType) // [1..] to remove the leading dot
        ? extType
        : OutputType.Unknown;
}
