// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Core;

namespace Tests.Portamical.Companion.Core;

[TestClass]
public class SpecSetTests
{
    private static TestCaseSpec Spec(string definition, string expected = "5") => new()
    {
        Definition = definition,
        Kind = ResultKind.Returns,
        ExpectedTypeName = "int",
        ExpectedValueLiteral = expected,
        ExpectedDisplay = expected,
    };

    [TestMethod]
    public void Distinct_duplicateNames_keepsFirstOccurrence()
    {
        var specs = new[] { Spec("A"), Spec("B"), Spec("A"), Spec("A") };

        var result = SpecSet.Distinct(specs);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("A => returns 5", result[0].TestCaseName);
        Assert.AreEqual("B => returns 5", result[1].TestCaseName);
    }

    [TestMethod]
    public void Distinct_sameDefinitionDifferentResult_keepsBoth()
    {
        var specs = new[] { Spec("A", "5"), Spec("A", "6") };

        Assert.AreEqual(2, SpecSet.Distinct(specs).Count);
    }

    [TestMethod]
    public void Distinct_null_throwsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => SpecSet.Distinct(null!));
    }

    [TestMethod]
    public void AnalyzeGaps_partitionsCoveredAndMissing()
    {
        var existing = new[] { "A => returns 5" };
        var proposed = new[] { Spec("A"), Spec("B") };

        var result = SpecSet.AnalyzeGaps(existing, proposed);

        Assert.AreEqual(1, result.Covered.Count);
        Assert.AreEqual(1, result.Missing.Count);
        Assert.AreEqual("B => returns 5", result.Missing[0].TestCaseName);
    }

    [TestMethod]
    public void AnalyzeGaps_deduplicatesProposalsFirst()
    {
        var result = SpecSet.AnalyzeGaps([], [Spec("A"), Spec("A")]);

        Assert.AreEqual(1, result.Missing.Count);
    }

    [TestMethod]
    public void AnalyzeGaps_emptyProposals_returnsEmptyPartitions()
    {
        var result = SpecSet.AnalyzeGaps(["A => returns 5"], []);

        Assert.AreEqual(0, result.Covered.Count);
        Assert.AreEqual(0, result.Missing.Count);
    }
}
