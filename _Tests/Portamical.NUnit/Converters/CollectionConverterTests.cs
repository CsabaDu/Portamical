// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Patterns;
using Portamical.NUnit.Converters;
using Portamical.NUnit.TestDataTypes;

namespace Tests.Portamical.NUnit.Converters;

[TestClass]
public class CollectionConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    // ToTestCaseDataCollection

    [TestMethod]
    public void ToTestCaseDataCollection_singleElement_hasOneItem()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_multipleDistinct_hasAllItems()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [CreateData("same"), dup];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_returnsIReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<global::NUnit.Framework.TestCaseData>>(result);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_withMethodName_setsTestNameWithPrefix()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance, "TestMethod");
        var first = result.First();
        Assert.IsNotNull(first.TestName);
        Assert.StartsWith("TestMethod", first.TestName!);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_withoutMethodName_testNameIsTestCaseName()
    {
        var item = CreateData("myCaseName");
        ITestData[] collection = [item];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance);
        var first = result.First();
        Assert.AreEqual(item.TestCaseName, first.TestName);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_argsCodeInstance_firstArgIsTestData()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Instance);
        var first = result.First();
        Assert.AreSame(item, first.Arguments![0]);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_argsCodeProperties_argumentsAreFlattened()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = collection.ToTestCaseDataCollection(ArgsCode.Properties);
        var first = result.First();
        var expectedArgs = item.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        CollectionAssert.AreEqual(expectedArgs, first.Arguments);
    }

    [TestMethod]
    public void ToTestCaseDataCollection_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTestCaseDataCollection(ArgsCode.Instance));

    [TestMethod]
    public void ToTestCaseDataCollection_nullCollection_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTestCaseDataCollection(ArgsCode.Instance));

    // ToTestCaseTestDataCollection

    [TestMethod]
    public void ToTestCaseTestDataCollection_singleElement_hasOneItem()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_multipleDistinct_hasAllItems()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_returnsIReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<TestCaseTestData<ITestData>>>(result);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_setsTestCaseName()
    {
        var item = CreateData("myCaseName");
        ITestData[] collection = [item];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance);
        Assert.AreEqual(item.TestCaseName, result.First().TestCaseName);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_withMethodName_setsTestNameWithPrefix()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance, "TestMethod");
        Assert.StartsWith("TestMethod", result.First().TestName!);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_argsCodeInstance_firstArgIsTestData()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = collection.ToTestCaseTestDataCollection(ArgsCode.Instance);
        Assert.AreSame(item, result.First().Arguments![0]);
    }

    [TestMethod]
    public void ToTestCaseTestDataCollection_emptyCollection_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToTestCaseTestDataCollection(ArgsCode.Instance));

    [TestMethod]
    public void ToTestCaseTestDataCollection_nullCollection_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<ITestData>)null!).ToTestCaseTestDataCollection(ArgsCode.Instance));
}
