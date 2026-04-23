// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.TestBases.ObjectArrayCollection;

namespace Tests.Portamical.TestBases.ObjectArrayCollection;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteBase : TestBase
    {
        public static IReadOnlyCollection<object?[]> InvokeConvert<TTestData>(
            IEnumerable<TTestData> collection, ArgsCode argsCode)
        where TTestData : notnull, ITestData
            => Convert(collection, argsCode);

        public static IReadOnlyCollection<object?[]> InvokeConvertDefault<TTestData>(
            IEnumerable<TTestData> collection)
        where TTestData : notnull, ITestData
            => Convert(collection);
    }

#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region Convert(collection, argsCode)

    [TestMethod]
    public void Convert_argsCodeInstance_returnsWrappedTestDataPerRow()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.HasCount(1, result);
        var row = result.Single();
        Assert.HasCount(1, row);
        Assert.AreSame(item, row[0]);
    }

    [TestMethod]
    public void Convert_argsCodeProperties_returnsFlattenedProperties()
    {
        var item = CreateData("p");
        ITestData[] collection = [item];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Properties);
        var expected = item.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(expected, result.Single());
    }

    [TestMethod]
    public void Convert_argsCode_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), duplicate];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_argsCode_multipleDistinct_returnsAll()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Properties);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void Convert_argsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ConcreteBase.InvokeConvert(nullCollection, ArgsCode.Instance));
    }

    [TestMethod]
    public void Convert_argsCode_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => ConcreteBase.InvokeConvert(Array.Empty<ITestData>(), ArgsCode.Instance));
    }

    #endregion

    #region Convert(collection) — default overload

    [TestMethod]
    public void Convert_default_usesArgsCodeInstance()
    {
        var item = CreateData("d");
        ITestData[] collection = [item];
        var explicitResult = ConcreteBase.InvokeConvert(collection, ArgsCode.Instance);
        var defaultResult = ConcreteBase.InvokeConvertDefault(collection);
        CollectionAssert.AreEqual(explicitResult.Single(), defaultResult.Single());
    }

    [TestMethod]
    public void Convert_default_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("def-dup", "result", 77);
        ITestData[] collection = [CreateData("def-dup"), duplicate];
        var result = ConcreteBase.InvokeConvertDefault(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_default_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ConcreteBase.InvokeConvertDefault(nullCollection));
    }

    [TestMethod]
    public void Convert_default_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => ConcreteBase.InvokeConvertDefault(Array.Empty<ITestData>()));
    }

    #endregion
}
