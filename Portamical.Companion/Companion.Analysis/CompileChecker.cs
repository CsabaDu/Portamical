// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Portamical.Companion.Analysis;

/// <summary>
/// Result of an in-memory compile check of emitted code.
/// </summary>
/// <param name="Success">Whether compilation produced no errors.</param>
/// <param name="Errors">Formatted error diagnostics, if any.</param>
public sealed record CompileCheckResult(bool Success, IReadOnlyList<string> Errors);

/// <summary>
/// In-memory Roslyn compile check — the mechanical verification gate for emitted test code.
/// Generated code is compiled against the current runtime plus caller-supplied reference
/// assemblies (e.g. Portamical.Core.dll) before being presented to the user.
/// </summary>
public static class CompileChecker
{
    /// <summary>
    /// Compiles the given source texts in memory and returns error diagnostics.
    /// </summary>
    /// <param name="sourceTexts">Source files to compile together.</param>
    /// <param name="referencePaths">Paths of additional reference assemblies (e.g. Portamical.Core.dll).</param>
    public static CompileCheckResult Check(
        IEnumerable<string> sourceTexts,
        IEnumerable<string>? referencePaths = null)
    {
        ArgumentNullException.ThrowIfNull(sourceTexts);

        var trees = sourceTexts
            .Select(s => CSharpSyntaxTree.ParseText(s))
            .ToList();

        var references = GetRuntimeReferences()
            .Concat((referencePaths ?? [])
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)))
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "Portamical.Companion.CompileCheck",
            syntaxTrees: trees,
            references: references,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToList();

        return new CompileCheckResult(errors.Count == 0, errors);
    }

    private static IEnumerable<MetadataReference> GetRuntimeReferences()
    {
        string? trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

        if (string.IsNullOrEmpty(trustedAssemblies))
        {
            yield break;
        }

        foreach (string path in trustedAssemblies.Split(Path.PathSeparator))
        {
            string fileName = Path.GetFileName(path);

            // Core runtime surface is enough for compile checks; skip niche assemblies.
            if (fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                || fileName is "mscorlib.dll" or "netstandard.dll" or "System.dll")
            {
                yield return MetadataReference.CreateFromFile(path);
            }
        }
    }
}
