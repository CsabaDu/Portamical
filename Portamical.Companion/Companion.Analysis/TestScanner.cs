// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Portamical.Companion.Core;

namespace Portamical.Companion.Analysis;

/// <summary>
/// Scans existing test source files for Portamical test case names — string literals
/// containing the <c>" =&gt; "</c> separator — enabling dedup-aware gap analysis.
/// </summary>
public static class TestScanner
{
    /// <summary>
    /// Extracts all distinct test case names ("definition =&gt; result" string literals)
    /// from the given C# source text.
    /// </summary>
    public static IReadOnlyList<string> ExtractTestCaseNames(string sourceText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);

        var tree = CSharpSyntaxTree.ParseText(sourceText);
        var root = tree.GetRoot();

        return [.. root.DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
            .Select(l => l.Token.ValueText)
            .Where(IsTestCaseName)
            .Distinct(StringComparer.Ordinal)];
    }

    /// <summary>
    /// Extracts test case names from multiple source files.
    /// </summary>
    public static IReadOnlyList<string> ExtractTestCaseNames(IEnumerable<string> sourceTexts)
    {
        ArgumentNullException.ThrowIfNull(sourceTexts);

        return [.. sourceTexts
            .SelectMany(ExtractTestCaseNames)
            .Distinct(StringComparer.Ordinal)];
    }

    private static bool IsTestCaseName(string value)
    => NamingSemantics.TryParse(value, out _);
}
