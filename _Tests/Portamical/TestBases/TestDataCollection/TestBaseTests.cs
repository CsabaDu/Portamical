// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using Portamical.TestBases.TestDataCollection;

namespace Tests.Portamical.TestBases.TestDataCollection;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteBase : TestBase
    {
        public static IReadOnlyCollection<TTestData> InvokeConvert<TTestData>(
            IEnumerable<TTestData> collection)
        where TTestData : notnull, ITestData
            => Convert(collection);
    }

#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region Convert

    [TestMethod]
    public void Convert_singleElement_returnsCollectionOfOne()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_multipleDistinct_returnsAllElements()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void Convert_deduplicates_keepsFirstOccurrence()
    {
        var first = CreateData("same");
        var duplicate = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [first, duplicate];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.HasCount(1, result);
        Assert.AreSame(first, result.Single());
    }

    [TestMethod]
    public void Convert_returnsOriginalTestDataInstances()
    {
        var item = CreateData("item");
        ITestData[] collection = [item];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.AreSame(item, result.Single());
    }

    [TestMethod]
    public void Convert_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ConcreteBase.InvokeConvert(nullCollection));
    }

    [TestMethod]
    public void Convert_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => ConcreteBase.InvokeConvert(Array.Empty<ITestData>()));
    }

    #endregion
}
