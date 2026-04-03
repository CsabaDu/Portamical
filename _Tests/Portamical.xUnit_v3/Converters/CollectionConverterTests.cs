// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes;

namespace Tests.Portamical.xUnit_v3.Converters;

[TestClass]
public class CollectionConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    // ToTheoryTestData

    [TestMethod]
    public void ToTheoryTestData_singleElement_hasOneRow()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestData(ArgsCode.Instance);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ToTheoryTestData_multipleDistinct_hasAllRows()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToTheoryTestData(ArgsCode.Instance);
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void ToTheoryTestData_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [CreateData("same"), dup];
        var result = collection.ToTheoryTestData(ArgsCode.Instance);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ToTheoryTestData_returnsCorrectType()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestData(ArgsCode.Instance);
        Assert.IsInstanceOfType<TheoryTestData<ITestData>>(result);
    }

    [TestMethod]
    public void ToTheoryTestData_setsArgsCode()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestData(ArgsCode.Properties);
        Assert.AreEqual(ArgsCode.Properties, result.ArgsCode);
    }

    [TestMethod]
    public void ToTheoryTestData_withTestMethodName_setsTestMethodName()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestData(ArgsCode.Instance, "TestMethod");
        Assert.AreEqual("TestMethod", result.TestMethodName);
    }

    [TestMethod]
    public void ToTheoryTestData_withTestMethodName_setsDisplayNameOnRow()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestData(ArgsCode.Instance, "TestMethod");
        var row = result.Single();
        Assert.Contains("TestMethod", row.TestDisplayName!);
    }

    [TestMethod]
    public void ToTheoryTestData_nullTestMethodName_rowDisplayNameIsNull()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = collection.ToTheoryTestData(ArgsCode.Instance, null);
        var row = result.Single();
        Assert.IsNull(row.TestDisplayName);
    }

    [TestMethod]
    public void ToTheoryTestData_nullCollection_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTheoryTestData(ArgsCode.Instance));

    [TestMethod]
    public void ToTheoryTestData_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTheoryTestData(ArgsCode.Instance));

    // ToTheoryTestDataRowCollection

    [TestMethod]
    public void ToTheoryTestDataRowCollection_singleElement_hasOneRow()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestDataRowCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTheoryTestDataRowCollection_multipleDistinct_hasAllRows()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToTheoryTestDataRowCollection(ArgsCode.Instance);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToTheoryTestDataRowCollection_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [CreateData("same"), dup];
        var result = collection.ToTheoryTestDataRowCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTheoryTestDataRowCollection_returnsIReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestDataRowCollection(ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<ITheoryTestDataRow>>(result);
    }

    [TestMethod]
    public void ToTheoryTestDataRowCollection_withTestMethodName_setsDisplayName()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryTestDataRowCollection(ArgsCode.Instance, "TestMethod");
        var row = result.Single();
        Assert.Contains("TestMethod", row.TestDisplayName!);
    }

    [TestMethod]
    public void ToTheoryTestDataRowCollection_nullCollection_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTheoryTestDataRowCollection(ArgsCode.Instance));

    [TestMethod]
    public void ToTheoryTestDataRowCollection_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTheoryTestDataRowCollection(ArgsCode.Instance));

    // ToTheoryDataRowCollection

    [TestMethod]
    public void ToTheoryDataRowCollection_singleElement_hasOneRow()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryDataRowCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTheoryDataRowCollection_multipleDistinct_hasAllRows()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToTheoryDataRowCollection(ArgsCode.Instance);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToTheoryDataRowCollection_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [CreateData("same"), dup];
        var result = collection.ToTheoryDataRowCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTheoryDataRowCollection_returnsIReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryDataRowCollection(ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<Xunit.ITheoryDataRow>>(result);
    }

    [TestMethod]
    public void ToTheoryDataRowCollection_withTestMethodName_setsDisplayName()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTheoryDataRowCollection(ArgsCode.Instance, "TestMethod");
        var row = result.Single();
        Assert.Contains("TestMethod", row.TestDisplayName!);
    }

    [TestMethod]
    public void ToTheoryDataRowCollection_nullCollection_throwsArgumentNullException()

        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTheoryDataRowCollection(ArgsCode.Instance));

    [TestMethod]
    public void ToTheoryDataRowCollection_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTheoryDataRowCollection(ArgsCode.Instance));
}
