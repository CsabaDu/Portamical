// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit_v3.TestBases.TheoryDataRowCollection;

namespace Tests.Portamical.xUnit_v3.TestBases.TheoryDataRowCollection;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteTestBase : TestBase
    {
        public static IReadOnlyCollection<Xunit.ITheoryDataRow> ConvertWith<TTestData>(
            IEnumerable<TTestData> data, ArgsCode argsCode, string? name = null)
            where TTestData : notnull, ITestData
            => Convert(data, argsCode, name);

        public static IReadOnlyCollection<Xunit.ITheoryDataRow> ConvertAsInstance<TTestData>(
            IEnumerable<TTestData> data, string? name = null)
            where TTestData : notnull, ITestData
            => Convert(data, name);
    }

    private static ITestData CreateData(string def)
        => TestDataFactory.CreateTestData<int>(def, "result", 1);

    [TestMethod]
    public void Convert_withArgsCode_returnsIReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<Xunit.ITheoryDataRow>>(result);
    }

    [TestMethod]
    public void Convert_withArgsCode_hasCorrectCount()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void Convert_withMethodName_setsDisplayNameOnRows()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance, "TestMethod");
        var row = result.Single();
        Assert.Contains("TestMethod", row.TestDisplayName!);
    }

    [TestMethod]
    public void Convert_asInstance_returnsRows()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertAsInstance(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_asInstance_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteTestBase.ConvertAsInstance(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_withArgsCode_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_instance_rowWrapsTestData()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        var row = result.Single();
        var data = row.GetData();
        Assert.HasCount(1, data);
        Assert.AreSame(item, data[0]);
    }

    [TestMethod]
    public void Convert_properties_rowWrapsArgsArray()
    {
        var item = CreateData("p");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Properties);
        var row = result.Single();
        var data = row.GetData();
        // TheoryDataRow(object) wraps the args array as a single element
        Assert.HasCount(1, data);
        Assert.IsInstanceOfType<object?[]>(data[0]);
        var innerArgs = (object?[])data[0]!;
        var sut_expected = item.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(sut_expected, innerArgs);
    }
}
