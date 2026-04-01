// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;

namespace Tests.Portamical.Core.Safety;

[TestClass]
public sealed class ValidatorTests
{
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
        int[] array = [1, 2, 3];

        var result = Validator.NotNullOrEmpty(array, nameof(array));

        Assert.AreSame(array, result);
    }

    [TestMethod]
    public void NotNullOrEmpty_nonArrayEnumerable_returnsEquivalentArray()
    {
        IEnumerable<int> source = Enumerable.Range(1, 3);

        var result = Validator.NotNullOrEmpty(source, nameof(source));

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
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
}
