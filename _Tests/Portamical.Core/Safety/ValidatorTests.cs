// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;

namespace Tests.Portamical.Core.Safety;

[TestClass]
public sealed class ValidatorTests
{
    private static readonly int[] s_testArray = [1, 2, 3];

    // NotNull

    [TestMethod]
    public void NotNull_nonNullReferenceType_returnsValue()
    {
        string value = "hello";

        var result = Validator.NotNull(value, nameof(value));

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void NotNull_nonNullValueType_returnsValue()
    {
        int? value = 42;

        var result = Validator.NotNull(value, nameof(value));

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void NotNull_nullReferenceType_throwsArgumentNullException()
    {
        string? value = null;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => Validator.NotNull(value, nameof(value)));
    }

    [TestMethod]
    public void NotNull_nullValueType_throwsArgumentNullException()
    {
        int? value = null;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => Validator.NotNull(value, nameof(value)));
    }

    [TestMethod]
    public void NotNull_nullValue_exceptionContainsParamName()
    {
        string? value = null;
        string paramName = nameof(value);

        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => Validator.NotNull(value, paramName));

        Assert.AreEqual(paramName, ex.ParamName);
    }

    // NotNullOrEmpty

    [TestMethod]
    public void NotNullOrEmpty_arrayInput_returnsSameInstance()
    {
        int[] array = s_testArray;

        var result = Validator.NotNullOrEmpty(array, nameof(array));

        Assert.AreSame(array, result);
    }

    [TestMethod]
    public void NotNullOrEmpty_nonArrayEnumerable_returnsEquivalentArray()
    {
        IEnumerable<int> source = Enumerable.Range(1, 3);

        var result = Validator.NotNullOrEmpty(source, nameof(source));

        CollectionAssert.AreEqual(s_testArray, result);
    }

    [TestMethod]
    public void NotNullOrEmpty_nullEnumerable_throwsArgumentNullException()
    {
        IEnumerable<int>? enumerable = null;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => Validator.NotNullOrEmpty(enumerable, nameof(enumerable)));
    }

    [TestMethod]
    public void NotNullOrEmpty_nullEnumerable_exceptionContainsParamName()
    {
        IEnumerable<string>? enumerable = null;
        string paramName = nameof(enumerable);

        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => Validator.NotNullOrEmpty(enumerable, paramName));

        Assert.AreEqual(paramName, ex.ParamName);
    }

    [TestMethod]
    public void NotNullOrEmpty_emptyEnumerable_throwsArgumentException()
    {
        IEnumerable<int> enumerable = [];

        Assert.ThrowsExactly<ArgumentException>(
            () => Validator.NotNullOrEmpty(enumerable, nameof(enumerable)));
    }

    [TestMethod]
    public void NotNullOrEmpty_emptyEnumerable_exceptionMessageIsCorrect()
    {
        IEnumerable<int> enumerable = [];
        string paramName = nameof(enumerable);

        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => Validator.NotNullOrEmpty(enumerable, paramName));

        Assert.AreEqual("The sequence must contain at least one element.", ex.Message.Split(" (Parameter")[0]);
    }

    [TestMethod]
    public void NotNullOrEmpty_emptyEnumerable_exceptionContainsParamName()
    {
        IEnumerable<int> enumerable = [];
        string paramName = nameof(enumerable);

        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => Validator.NotNullOrEmpty(enumerable, paramName));

        Assert.AreEqual(paramName, ex.ParamName);
    }

    #region SnapshotOrNull

    // SnapshotOrNull - Null Input

    [TestMethod]
    public void SnapshotOrNull_nullEnumerable_returnsNull()
    {
        IEnumerable<int>? enumerable = null;

        var result = Validator.SnapshotOrNull(enumerable);

        Assert.IsNull(result);
    }

    // SnapshotOrNull - Array Input (O(1) optimization)

    [TestMethod]
    public void SnapshotOrNull_arrayInput_returnsSameInstance()
    {
        int[] array = s_testArray;

        var result = Validator.SnapshotOrNull(array);

        Assert.AreSame(array, result, "Should return the same array instance without creating a new allocation.");
    }

    [TestMethod]
    public void SnapshotOrNull_emptyArray_returnsSameInstance()
    {
        int[] array = [];

        var result = Validator.SnapshotOrNull(array);

        Assert.AreSame(array, result);
    }

    // SnapshotOrNull - Non-Array IEnumerable (O(n) conversion)

    [TestMethod]
    public void SnapshotOrNull_listInput_createsArraySnapshot()
    {
        List<int> list = [1, 2, 3];

        var result = Validator.SnapshotOrNull(list);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<int[]>(result);
        CollectionAssert.AreEqual(s_testArray, result);
    }

    [TestMethod]
    public void SnapshotOrNull_enumerableRange_createsArraySnapshot()
    {
        IEnumerable<int> source = Enumerable.Range(1, 3);

        var result = Validator.SnapshotOrNull(source);

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(s_testArray, result);
    }

    [TestMethod]
    public void SnapshotOrNull_hashSetInput_createsArraySnapshot()
    {
        HashSet<string> hashSet = ["alpha", "beta", "gamma"];

        var result = Validator.SnapshotOrNull(hashSet);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Length);
        CollectionAssert.AreEquivalent(hashSet.ToArray(), result);
    }

    [TestMethod]
    public void SnapshotOrNull_emptyList_createsEmptyArray()
    {
        List<int> list = [];

        var result = Validator.SnapshotOrNull(list);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
    }

    // SnapshotOrNull - Prevention of Multiple Enumeration

    [TestMethod]
    public void SnapshotOrNull_yieldEnumerable_enumeratesOnlyOnce()
    {
        int enumerationCount = 0;
        IEnumerable<int> source = GetEnumerableWithCounter();

        var result = Validator.SnapshotOrNull(source);

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

        var result = Validator.SnapshotOrNull(array);

        Assert.AreSame(array, result);
    }

    [TestMethod]
    public void SnapshotOrNull_referenceTypeEnumerable_createsArraySnapshot()
    {
        IEnumerable<string> source = new List<string> { "alpha", "beta", "gamma" };

        var result = Validator.SnapshotOrNull(source);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Length);
        Assert.AreEqual("alpha", result[0]);
        Assert.AreEqual("beta", result[1]);
        Assert.AreEqual("gamma", result[2]);
    }

    [TestMethod]
    public void SnapshotOrNull_nullableValueTypeArray_returnsSameInstance()
    {
        int?[] array = [1, null, 3];

        var result = Validator.SnapshotOrNull(array);

        Assert.AreSame(array, result);
    }

    // SnapshotOrNull - Cast as IEnumerable

    [TestMethod]
    public void SnapshotOrNull_arrayCastAsIEnumerable_returnsSameInstance()
    {
        int[] array = s_testArray;
        IEnumerable<int> enumerable = array;

        var result = Validator.SnapshotOrNull(enumerable);

        Assert.AreSame(array, result, "Should recognize the underlying array and return it directly.");
    }

    #endregion
}
