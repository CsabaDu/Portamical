// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;

namespace Tests.Portamical.Core.Safety;

[TestClass, DoNotParallelize]
public sealed class ResolverTests
{
    [TestCleanup]
    public void Cleanup() => Resolver.ResetLogCounter();

    // FallbackIfNullOrWhiteSpace

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_validPreferredValue_returnsPreferredValue()
    {
        var result = "Fallback".FallbackIfNullOrWhiteSpace("Actual", "GetName");

        Assert.AreEqual("Actual", result);
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_nullPreferredValue_returnsFallbackWithIndex()
    {
        var result = "Fallback".FallbackIfNullOrWhiteSpace(null, "GetName");

        StringAssert.StartsWith(result, "Fallback (");
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_emptyPreferredValue_returnsFallbackWithIndex()
    {
        var result = "Fallback".FallbackIfNullOrWhiteSpace(string.Empty, "GetName");

        StringAssert.StartsWith(result, "Fallback (");
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_whitespacePreferredValue_returnsFallbackWithIndex()
    {
        var result = "Fallback".FallbackIfNullOrWhiteSpace("   ", "GetName");

        StringAssert.StartsWith(result, "Fallback (");
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_multipleFallbacks_indicesAreSequential()
    {
        var result1 = "Label".FallbackIfNullOrWhiteSpace(null, "GetName");
        var result2 = "Label".FallbackIfNullOrWhiteSpace(null, "GetName");
        var result3 = "Label".FallbackIfNullOrWhiteSpace(null, "GetName");

        var index1 = long.Parse(result1.Replace("Label (", "").TrimEnd(')'));
        var index2 = long.Parse(result2.Replace("Label (", "").TrimEnd(')'));
        var index3 = long.Parse(result3.Replace("Label (", "").TrimEnd(')'));

        Assert.AreEqual(index1 + 1, index2);
        Assert.AreEqual(index2 + 1, index3);
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_nullFallbackLabel_throwsArgumentNullException()
    {
        string? fallbackLabel = null;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => fallbackLabel!.FallbackIfNullOrWhiteSpace("value", "GetName"));
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_nullMethodName_throwsArgumentNullException()
    {
        string? methodName = null;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => "Fallback".FallbackIfNullOrWhiteSpace(null, methodName!));
    }

    // ResetLogCounter

    [TestMethod]
    public void ResetLogCounter_afterFallbacks_returnsPreviousCountAndResetsToZero()
    {
        "A".FallbackIfNullOrWhiteSpace(null, "M");
        "A".FallbackIfNullOrWhiteSpace(null, "M");
        "A".FallbackIfNullOrWhiteSpace(null, "M");

        var previousCount = Resolver.ResetLogCounter();

        Assert.AreEqual(3L, previousCount);
    }

    [TestMethod]
    public void ResetLogCounter_afterReset_counterStartsFromZero()
    {
        "A".FallbackIfNullOrWhiteSpace(null, "M");
        Resolver.ResetLogCounter();

        var result = "Label".FallbackIfNullOrWhiteSpace(null, "M");
        var index = long.Parse(result.Replace("Label (", "").TrimEnd(')'));

        Assert.AreEqual(1L, index);
    }
}
