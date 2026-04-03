// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestBases;

namespace Tests.Portamical.xUnit_v3.TestBases;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteTestBase : TestBase
    {
        public static TheoryTestData<TTestData> ConvertWith<TTestData>(
            IEnumerable<TTestData> data, ArgsCode argsCode, string? name = null)
            where TTestData : notnull, ITestData
            => Convert(data, argsCode, name);

        public static TheoryTestData<TTestData> ConvertAsInstance<TTestData>(
            IEnumerable<TTestData> data, string? name = null)
            where TTestData : notnull, ITestData
            => Convert(data, name);
    }

    private static ITestData CreateData(string def)
        => TestDataFactory.CreateTestData<int>(def, "result", 1);

    [TestMethod]
    public void Convert_withArgsCode_returnsTheoryTestData()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        Assert.IsInstanceOfType<TheoryTestData<ITestData>>(result);
    }

    [TestMethod]
    public void Convert_withArgsCode_setsArgsCode()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Properties);
        Assert.AreEqual(ArgsCode.Properties, result.ArgsCode);
    }

    [TestMethod]
    public void Convert_withArgsCode_hasCorrectCount()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void Convert_withMethodName_setsTestMethodName()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance, "TestMethod");
        Assert.AreEqual("TestMethod", result.TestMethodName);
    }

    [TestMethod]
    public void Convert_asInstance_usesInstanceArgsCode()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.ConvertAsInstance(collection);
        Assert.AreEqual(ArgsCode.Instance, result.ArgsCode);
    }

    [TestMethod]
    public void Convert_asInstance_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteTestBase.ConvertAsInstance(collection);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void Convert_withArgsCode_deduplicates()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void Convert_instance_rowWrapsTestData()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Instance);
        var row = result.Single();
        var data = ((Xunit.ITheoryDataRow)row).GetData();
        Assert.HasCount(1, data);
        Assert.AreSame(item, data[0]);
    }

    [TestMethod]
    public void Convert_properties_rowIsFlattenedArgs()
    {
        var item = CreateData("p");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.ConvertWith(collection, ArgsCode.Properties);
        var row = result.Single();
        var data = ((Xunit.ITheoryDataRow)row).GetData();
        var sut_expected = item.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(sut_expected, data);
    }
}
