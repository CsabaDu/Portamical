// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Companion.Core;

/// <summary>
/// Set-level operations over test case specs: deduplication (by <c>TestCaseName</c>,
/// matching Portamical's <c>INamedCase</c> identity semantics) and gap analysis against
/// existing test case names.
/// </summary>
public static class SpecSet
{
    /// <summary>
    /// Removes duplicate specs based on ordinal <see cref="TestCaseSpec.TestCaseName"/> equality,
    /// keeping first occurrences (mirrors <c>CollectionConverter.ToDistinctArray</c>).
    /// </summary>
    public static IReadOnlyList<TestCaseSpec> Distinct(IEnumerable<TestCaseSpec> specs)
    {
        ArgumentNullException.ThrowIfNull(specs);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<TestCaseSpec>();

        foreach (var spec in specs)
        {
            if (seen.Add(spec.TestCaseName))
            {
                result.Add(spec);
            }
        }

        return result;
    }

    /// <summary>
    /// Splits proposed specs into those already covered by existing test case names
    /// and those missing (the coverage gap).
    /// </summary>
    public static GapAnalysisResult AnalyzeGaps(
        IEnumerable<string> existingTestCaseNames,
        IEnumerable<TestCaseSpec> proposed)
    {
        ArgumentNullException.ThrowIfNull(existingTestCaseNames);
        ArgumentNullException.ThrowIfNull(proposed);

        var existing = new HashSet<string>(existingTestCaseNames, StringComparer.Ordinal);
        var covered = new List<TestCaseSpec>();
        var missing = new List<TestCaseSpec>();

        foreach (var spec in Distinct(proposed))
        {
            (existing.Contains(spec.TestCaseName) ? covered : missing).Add(spec);
        }

        return new GapAnalysisResult(covered, missing);
    }
}

/// <summary>
/// Result of a gap analysis: proposed specs partitioned into covered and missing.
/// </summary>
/// <param name="Covered">Specs whose test case name already exists in the test suite.</param>
/// <param name="Missing">Specs not yet covered by any existing test case.</param>
public sealed record GapAnalysisResult(
    IReadOnlyList<TestCaseSpec> Covered,
    IReadOnlyList<TestCaseSpec> Missing);
