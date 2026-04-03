// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit.Converters;
using Portamical.xUnit.DataProviders;

namespace Tests.Portamical.xUnit.Converters;

[TestClass]
public class CollectionConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    // ToTheoryData

    [TestMethod]
    public void ToTheoryData_singleElement_hasOneRow()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryData();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTheoryData_multipleDistinct_hasAllRows()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToTheoryData();
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToTheoryData_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [CreateData("same"), dup];
        var result = collection.ToTheoryData();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTheoryData_rowContainsOriginalInstance()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = collection.ToTheoryData();
        Assert.AreSame(item, result.Cast<object?[]>().First()[0]);
    }

    [TestMethod]
    public void ToTheoryData_returnsCorrectType()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryData();
        Assert.IsInstanceOfType<global::Xunit.TheoryData<ITestData>>(result);
    }

    [TestMethod]
    public void ToTheoryData_nullCollection_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTheoryData());

    [TestMethod]
    public void ToTheoryData_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTheoryData());

    // ToTestDataProvider

    [TestMethod]
    public void ToTestDataProvider_singleElement_returnsCorrectType()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestDataProvider(ArgsCode.Instance);
        Assert.IsInstanceOfType<TestDataProvider<ITestData>>(result);
    }

    [TestMethod]
    public void ToTestDataProvider_argsCodeInstance_storedOnProvider()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestDataProvider(ArgsCode.Instance);
        Assert.AreEqual(ArgsCode.Instance, result.ArgsCode);
    }

    [TestMethod]
    public void ToTestDataProvider_argsCodeProperties_storedOnProvider()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestDataProvider(ArgsCode.Properties);
        Assert.AreEqual(ArgsCode.Properties, result.ArgsCode);
    }

    [TestMethod]
    public void ToTestDataProvider_multipleElements_allRowsEnumerated()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var provider = collection.ToTestDataProvider(ArgsCode.Instance);
        var rows = provider.Cast<object?[]>().ToList();
        Assert.HasCount(3, rows);
    }

    [TestMethod]
    public void ToTestDataProvider_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var provider = collection.ToTestDataProvider(ArgsCode.Instance);
        var rows = provider.Cast<object?[]>().ToList();
        Assert.HasCount(1, rows);
    }

    [TestMethod]
    public void ToTestDataProvider_nullCollection_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTestDataProvider(ArgsCode.Instance));

    [TestMethod]
    public void ToTestDataProvider_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTestDataProvider(ArgsCode.Instance));
}
