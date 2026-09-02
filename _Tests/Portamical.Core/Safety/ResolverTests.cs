// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;

namespace Tests.Portamical.Core.Safety;

[TestClass, DoNotParallelize]
public sealed class ResolverTests
{
    private static readonly int[] s_testArray = [1, 2, 3];

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

        Assert.StartsWith("Fallback (", result);
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_emptyPreferredValue_returnsFallbackWithIndex()
    {
        var result = "Fallback".FallbackIfNullOrWhiteSpace(string.Empty, "GetName");

        Assert.StartsWith("Fallback (", result);
    }

    [TestMethod]
    public void FallbackIfNullOrWhiteSpace_whitespacePreferredValue_returnsFallbackWithIndex()
    {
        var result = "Fallback".FallbackIfNullOrWhiteSpace("   ", "GetName");

        Assert.StartsWith("Fallback (", result);
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


    #region SnapshotOrNull

    // SnapshotOrNull - Null Input

    [TestMethod]
    public void SnapshotOrNull_nullEnumerable_returnsNull()
    {
        IEnumerable<int>? enumerable = null;

        var result = Resolver.SnapshotOrNull(enumerable);

        Assert.IsNull(result);
    }

    // SnapshotOrNull - Array Input (O(1) optimization)

    [TestMethod]
    public void SnapshotOrNull_arrayInput_returnsSameInstance()
    {
        int[] array = s_testArray;

        var result = Resolver.SnapshotOrNull(array);

        Assert.AreSame(array, result, "Should return the same array instance without creating a new allocation.");
    }

    [TestMethod]
    public void SnapshotOrNull_emptyArray_returnsSameInstance()
    {
        int[] array = [];

        var result = Resolver.SnapshotOrNull(array);

        Assert.AreSame(array, result);
    }

    // SnapshotOrNull - Non-Array IEnumerable (O(n) conversion)

    [TestMethod]
    public void SnapshotOrNull_listInput_createsArraySnapshot()
    {
        List<int> list = [1, 2, 3];

        var result = Resolver.SnapshotOrNull(list);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<int[]>(result);
        CollectionAssert.AreEqual(s_testArray, result);
    }

    [TestMethod]
    public void SnapshotOrNull_enumerableRange_createsArraySnapshot()
    {
        IEnumerable<int> source = Enumerable.Range(1, 3);

        var result = Resolver.SnapshotOrNull(source);

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(s_testArray, result);
    }

    [TestMethod]
    public void SnapshotOrNull_hashSetInput_createsArraySnapshot()
    {
        HashSet<string> hashSet = ["alpha", "beta", "gamma"];

        var result = Resolver.SnapshotOrNull(hashSet);

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
        CollectionAssert.AreEquivalent(hashSet.ToArray(), result);
    }

    [TestMethod]
    public void SnapshotOrNull_emptyList_createsEmptyArray()
    {
        List<int> list = [];

        var result = Resolver.SnapshotOrNull(list);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    // SnapshotOrNull - Prevention of Multiple Enumeration

    [TestMethod]
    public void SnapshotOrNull_yieldEnumerable_enumeratesOnlyOnce()
    {
        int enumerationCount = 0;
        IEnumerable<int> source = GetEnumerableWithCounter();

        var result = Resolver.SnapshotOrNull(source);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, enumerationCount, "Source should be enumerated exactly once.");
        CollectionAssert.AreEqual(s_testArray, result);

        IEnumerable<int> GetEnumerableWithCounter()
        {
            enumerationCount++;
            foreach (var item in s_testArray)
            {
                yield return item;
            }
        }
    }

    // SnapshotOrNull - Type Variations

    [TestMethod]
    public void SnapshotOrNull_stringArray_returnsSameInstance()
    {
        string[] array = ["alpha", "beta", "gamma"];

        var result = Resolver.SnapshotOrNull(array);

        Assert.AreSame(array, result);
    }

    [TestMethod]
    public void SnapshotOrNull_referenceTypeEnumerable_createsArraySnapshot()
    {
        IEnumerable<string> source = ["alpha", "beta", "gamma"];

        var result = Resolver.SnapshotOrNull(source);

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
        Assert.AreEqual("alpha", result[0]);
        Assert.AreEqual("beta", result[1]);
        Assert.AreEqual("gamma", result[2]);
    }

    [TestMethod]
    public void SnapshotOrNull_nullableValueTypeArray_returnsSameInstance()
    {
        int?[] array = [1, null, 3];

        var result = Resolver.SnapshotOrNull(array);

        Assert.AreSame(array, result);
    }

    // SnapshotOrNull - Cast as IEnumerable

    [TestMethod]
    public void SnapshotOrNull_arrayCastAsIEnumerable_returnsSameInstance()
    {
        int[] array = s_testArray;
        IEnumerable<int> enumerable = array;

        var result = Resolver.SnapshotOrNull(enumerable);

        Assert.AreSame(array, result, "Should recognize the underlying array and return it directly.");
    }

    #endregion

}
