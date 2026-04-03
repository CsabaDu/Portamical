// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Collections;
using System.ComponentModel;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.DataProviders;
using Portamical.xUnit.DataProviders;

namespace Tests.Portamical.xUnit.DataProviders;

[TestClass]
public class TestDataProviderTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void Constructor_setsArgsCode_instance()
    {
        var provider = new TestDataProvider<ITestData>(CreateData("a"), ArgsCode.Instance);
        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
    }

    [TestMethod]
    public void Constructor_setsArgsCode_properties()
    {
        var provider = new TestDataProvider<ITestData>(CreateData("a"), ArgsCode.Properties);
        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
    }

    [TestMethod]
    public void Constructor_invalidArgsCode_throwsInvalidEnumArgumentException()
    {
        var testData = CreateData("a");
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => new TestDataProvider<ITestData>(testData, (ArgsCode)99));
    }

    [TestMethod]
    public void Constructor_addsFirstRowImmediately()
    {
        var testData = CreateData("first");
        var provider = new TestDataProvider<ITestData>(testData, ArgsCode.Instance);
        var rows = provider.Cast<object?[]>().ToList();
        Assert.HasCount(1, rows);
    }

    [TestMethod]
    public void TestMethodName_defaultsToNull()
    {
        var provider = new TestDataProvider<ITestData>(CreateData("a"), ArgsCode.Instance);
        Assert.IsNull(provider.TestMethodName);
    }

    [TestMethod]
    public void ImplementsIEnumerable()
    {
        var provider = new TestDataProvider<ITestData>(CreateData("a"), ArgsCode.Instance);
        Assert.IsInstanceOfType<IEnumerable>(provider);
    }

    [TestMethod]
    public void ImplementsITestDataProvider()
    {
        var provider = new TestDataProvider<ITestData>(CreateData("a"), ArgsCode.Instance);
        Assert.IsInstanceOfType<ITestDataProvider<ITestData>>(provider);
    }

    [TestMethod]
    public void AddRow_instance_appendsRow()
    {
        var first = CreateData("first");
        var second = CreateData("second");
        var provider = new TestDataProvider<ITestData>(first, ArgsCode.Instance);
        provider.AddRow(second);
        var rows = provider.Cast<object?[]>().ToList();
        Assert.HasCount(2, rows);
    }

    [TestMethod]
    public void GetEnumerator_instance_rowWrapsTestDataInObjectArray()
    {
        var testData = CreateData("x");
        var provider = new TestDataProvider<ITestData>(testData, ArgsCode.Instance);
        var row = provider.Cast<object?[]>().Single();
        Assert.HasCount(1, row);
        Assert.AreSame(testData, row[0]);
    }

    [TestMethod]
    public void GetEnumerator_properties_rowIsFlattenedArgs()
    {
        var testData = CreateData("p");
        var provider = new TestDataProvider<ITestData>(testData, ArgsCode.Properties);
        var row = provider.Cast<object?[]>().Single();
        var expectedRow = testData.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(expectedRow, row);
    }

    [TestMethod]
    public void AddRow_properties_appendsFlattenedArgs()
    {
        var first = CreateData("first");
        var second = CreateData("second");
        var provider = new TestDataProvider<ITestData>(first, ArgsCode.Properties);
        provider.AddRow(second);
        var rows = provider.Cast<object?[]>().ToList();
        var expectedRow = second.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(expectedRow, rows[1]);
    }
}
