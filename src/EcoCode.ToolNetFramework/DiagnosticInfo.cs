﻿using Microsoft.CodeAnalysis;

namespace EcoCode.ToolNetFramework;

internal readonly struct DiagnosticInfo
{
    public string Path { get; }
    public string File { get; }
    public string Location { get; }
    public string Severity { get; }
    public string Code { get; }
    public string Message { get; }

    private DiagnosticInfo(string path, string file, string location, string severity, string code, string message)
    {
        Path = path;
        File = file;
        Location = location;
        Severity = severity;
        Code = code;
        Message = message;
    }

    public static DiagnosticInfo FromDiagnostic(Diagnostic diagnostic)
    {
        var pos = diagnostic.Location.GetLineSpan();
        return new DiagnosticInfo(
            System.IO.Path.GetDirectoryName(pos.Path),
            System.IO.Path.GetFileName(pos.Path),
            $"Row {pos.StartLinePosition.Line + 1}, Column {pos.StartLinePosition.Character + 1}",
            diagnostic.Severity.ToString(),
            diagnostic.Id,
            diagnostic.GetMessage());
    }
}