// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit.TestBases.TheoryData;

namespace Tests.Portamical.xUnit.TestBases.TheoryData;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteBase : TestBase
    {
        public static global::Xunit.TheoryData<TTestData> InvokeConvert<TTestData>(
            IEnumerable<TTestData> collection)
        where TTestData : notnull, ITestData
            => Convert(collection);
    }

    private static ITestData CreateData(string def)
        => TestDataFactory.CreateTestData<int>(def, "result", 1);

    [TestMethod]
    public void Convert_returnsCorrectType()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.IsInstanceOfType<global::Xunit.TheoryData<ITestData>>(result);
    }

    [TestMethod]
    public void Convert_singleElement_hasOneRow()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_rowContainsOriginalInstance()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.AreSame(item, result.Cast<object?[]>().First()[0]);
    }

    [TestMethod]
    public void Convert_duplicateTestCaseName_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_multipleElements_allRowsIncluded()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = ConcreteBase.InvokeConvert(collection);
        Assert.HasCount(3, result);
    }
}
