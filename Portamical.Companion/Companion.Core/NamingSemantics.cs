// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Companion.Core;

/// <summary>
/// Single source of truth for the Portamical "definition =&gt; result" test case name semantics.
/// Mirrors the behavior of <c>Portamical.Core.TestDataTypes.Models.TestDataBase</c>:
/// separator <c>" =&gt; "</c>, result prefixes <c>"returns"</c> and <c>"throws"</c>.
/// </summary>
public static class NamingSemantics
{
    /// <summary>The separator between definition and result.</summary>
    public const string Separator = " => ";

    /// <summary>Result prefix used by <c>TestDataReturns</c>.</summary>
    public const string ReturnsPrefix = "returns";

    /// <summary>Result prefix used by <c>TestDataThrows</c>.</summary>
    public const string ThrowsPrefix = "throws";

    /// <summary>
    /// Renders the full test case name of a spec: <c>"{definition} =&gt; {result}"</c>.
    /// </summary>
    public static string Render(TestCaseSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.Definition + Separator + RenderResult(spec);
    }

    /// <summary>
    /// Renders only the result part of a spec (right side of "=&gt;").
    /// </summary>
    public static string RenderResult(TestCaseSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.Kind switch
        {
            ResultKind.Returns => $"{ReturnsPrefix} {spec.ExpectedDisplay ?? spec.ExpectedValueLiteral}",
            ResultKind.Throws => $"{ThrowsPrefix} {spec.ExpectedTypeName}",
            _ => spec.ExpectedDisplay ?? string.Empty,
        };
    }

    /// <summary>
    /// Parses a one-line <c>"definition =&gt; result"</c> proposal into a partial
    /// <see cref="TestCaseSpec"/> (definition, kind, and display — argument details
    /// must be filled in separately).
    /// </summary>
    /// <returns><see langword="true"/> if the line contains the separator and a non-empty definition.</returns>
    public static bool TryParse(string? line, out TestCaseSpec? spec)
    {
        spec = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        int separatorIndex = line.IndexOf(Separator, StringComparison.Ordinal);

        if (separatorIndex <= 0)
        {
            return false;
        }

        string definition = line[..separatorIndex].Trim();
        string result = line[(separatorIndex + Separator.Length)..].Trim();

        if (definition.Length == 0)
        {
            return false;
        }

        if (result.StartsWith(ReturnsPrefix + " ", StringComparison.Ordinal))
        {
            spec = new TestCaseSpec
            {
                Definition = definition,
                Kind = ResultKind.Returns,
                ExpectedDisplay = result[(ReturnsPrefix.Length + 1)..].Trim(),
            };
        }
        else if (result.StartsWith(ThrowsPrefix + " ", StringComparison.Ordinal))
        {
            spec = new TestCaseSpec
            {
                Definition = definition,
                Kind = ResultKind.Throws,
                ExpectedTypeName = result[(ThrowsPrefix.Length + 1)..].Trim(),
            };
        }
        else
        {
            spec = new TestCaseSpec
            {
                Definition = definition,
                Kind = ResultKind.Custom,
                ExpectedDisplay = result,
            };
        }

        return true;
    }
}
