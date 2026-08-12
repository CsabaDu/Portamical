// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Core;

namespace Tests.Portamical.Companion.Core;

[TestClass]
public class NamingSemanticsTests
{
    private static TestCaseSpec ReturnsSpec => new()
    {
        Definition = "Adding two positives",
        Kind = ResultKind.Returns,
        ExpectedTypeName = "int",
        ExpectedValueLiteral = "5",
        ExpectedDisplay = "5",
    };

    private static TestCaseSpec ThrowsSpec => new()
    {
        Definition = "Null input",
        Kind = ResultKind.Throws,
        ExpectedTypeName = "ArgumentNullException",
        ExpectedValueLiteral = "new ArgumentNullException()",
    };

    private static TestCaseSpec CustomSpec => new()
    {
        Definition = "Process complex data",
        Kind = ResultKind.Custom,
        ExpectedDisplay = "succeeds with warnings",
    };

    [TestMethod]
    public void Render_returnsSpec_usesReturnsPrefix()
    {
        Assert.AreEqual("Adding two positives => returns 5", NamingSemantics.Render(ReturnsSpec));
    }

    [TestMethod]
    public void Render_throwsSpec_usesThrowsPrefixWithTypeName()
    {
        Assert.AreEqual("Null input => throws ArgumentNullException", NamingSemantics.Render(ThrowsSpec));
    }

    [TestMethod]
    public void Render_customSpec_usesDisplayVerbatim()
    {
        Assert.AreEqual("Process complex data => succeeds with warnings", NamingSemantics.Render(CustomSpec));
    }

    [TestMethod]
    public void Render_nullSpec_throwsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => NamingSemantics.Render(null!));
    }

    [TestMethod]
    public void TestCaseName_property_matchesRender()
    {
        Assert.AreEqual(NamingSemantics.Render(ReturnsSpec), ReturnsSpec.TestCaseName);
    }

    [TestMethod]
    public void TryParse_returnsLine_parsesKindAndDisplay()
    {
        bool parsed = NamingSemantics.TryParse("Adding two positives => returns 5", out var spec);

        Assert.IsTrue(parsed);
        Assert.AreEqual(ResultKind.Returns, spec!.Kind);
        Assert.AreEqual("Adding two positives", spec.Definition);
        Assert.AreEqual("5", spec.ExpectedDisplay);
    }

    [TestMethod]
    public void TryParse_throwsLine_parsesExceptionTypeName()
    {
        bool parsed = NamingSemantics.TryParse("Null input => throws ArgumentNullException", out var spec);

        Assert.IsTrue(parsed);
        Assert.AreEqual(ResultKind.Throws, spec!.Kind);
        Assert.AreEqual("ArgumentNullException", spec.ExpectedTypeName);
    }

    [TestMethod]
    public void TryParse_customLine_parsesAsCustom()
    {
        bool parsed = NamingSemantics.TryParse("Process data => succeeds with warnings", out var spec);

        Assert.IsTrue(parsed);
        Assert.AreEqual(ResultKind.Custom, spec!.Kind);
        Assert.AreEqual("succeeds with warnings", spec.ExpectedDisplay);
    }

    [TestMethod]
    public void TryParse_roundTrip_preservesTestCaseName()
    {
        string original = "Null input => throws ArgumentNullException";

        NamingSemantics.TryParse(original, out var spec);

        Assert.AreEqual(original, spec!.TestCaseName);
    }

    [TestMethod]
    public void TryParse_missingSeparator_returnsFalse()
    {
        Assert.IsFalse(NamingSemantics.TryParse("no separator here", out _));
    }

    [TestMethod]
    public void TryParse_emptyDefinition_returnsFalse()
    {
        Assert.IsFalse(NamingSemantics.TryParse(" => returns 5", out _));
    }

    [TestMethod]
    public void TryParse_nullOrWhitespace_returnsFalse()
    {
        Assert.IsFalse(NamingSemantics.TryParse(null, out _));
        Assert.IsFalse(NamingSemantics.TryParse("   ", out _));
    }
}
