// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Microsoft.CodeAnalysis;

namespace Portamical.Companion.Analysis;

/// <summary>
/// Shared discovery of the current runtime's trusted platform assemblies, used to build
/// in-memory Roslyn compilations for compile checks and semantic (symbol-based) analysis.
/// </summary>
internal static class RuntimeReferences
{
    /// <summary>
    /// Returns metadata references for the core runtime surface (System.* assemblies),
    /// which is enough to resolve BCL guard-clause and exception types.
    /// </summary>
    public static IEnumerable<MetadataReference> GetAll()
    {
        string? trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

        if (string.IsNullOrEmpty(trustedAssemblies))
        {
            yield break;
        }

        foreach (string path in trustedAssemblies.Split(Path.PathSeparator))
        {
            string fileName = Path.GetFileName(path);

            // Core runtime surface is enough for these purposes; skip niche assemblies.
            if (fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                || fileName is "mscorlib.dll" or "netstandard.dll" or "System.dll")
            {
                yield return MetadataReference.CreateFromFile(path);
            }
        }
    }
}
