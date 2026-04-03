// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit.DataProviders;
using Portamical.xUnit.TestBases;

namespace Tests.Portamical.xUnit.TestBases;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteBase : TestBase
    {
        public static TestDataProvider<TTestData> InvokeConvert<TTestData>(
            IEnumerable<TTestData> collection, ArgsCode argsCode)
        where TTestData : notnull, ITestData
            => Convert(collection, argsCode);

        public static TestDataProvider<TTestData> InvokeConvertDefault<TTestData>(
            IEnumerable<TTestData> collection)
        where TTestData : notnull, ITestData
            => Convert(collection);
    }

    private static ITestData CreateData(string def)
        => TestDataFactory.CreateTestData<int>(def, "result", 1);

    [TestMethod]
    public void Convert_withArgsCode_returnsTestDataProvider()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.IsInstanceOfType<TestDataProvider<ITestData>>(result);
    }

    [TestMethod]
    public void Convert_argsCodeInstance_rowIsWrappedTestData()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Instance);
        var row = result.Cast<object?[]>().Single();
        Assert.AreSame(item, row[0]);
    }

    [TestMethod]
    public void Convert_argsCodeProperties_rowIsFlattenedProperties()
    {
        var item = CreateData("p");
        ITestData[] collection = [item];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Properties);
        var row = result.Cast<object?[]>().Single();
        var expectedRow = item.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(expectedRow, row);
    }

    [TestMethod]
    public void Convert_default_usesArgsCodeInstance()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteBase.InvokeConvertDefault(collection);
        Assert.AreEqual(ArgsCode.Instance, result.ArgsCode);
    }

    [TestMethod]
    public void Convert_deduplicatesByTestCaseName()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteBase.InvokeConvert(collection, ArgsCode.Instance);
        var rows = result.Cast<object?[]>().ToList();
        Assert.HasCount(1, rows);
    }
}
