// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Tests.Portamical.xUnit_v3.DataProviders;

[TestClass]
public class TheoryTestDataTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void Constructor_setsArgsCode_instance()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, null);
        Assert.AreEqual(ArgsCode.Instance, sut.ArgsCode);
    }

    [TestMethod]
    public void Constructor_setsArgsCode_properties()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Properties, null);
        Assert.AreEqual(ArgsCode.Properties, sut.ArgsCode);
    }

    [TestMethod]
    public void Constructor_setsTestMethodName()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, "MyMethod");
        Assert.AreEqual("MyMethod", sut.TestMethodName);
    }

    [TestMethod]
    public void Constructor_nullTestMethodName_testMethodNameIsNull()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, null);
        Assert.IsNull(sut.TestMethodName);
    }

    [TestMethod]
    public void Constructor_addsFirstRow()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, null);
        Assert.AreEqual(1, sut.Count);
    }

    [TestMethod]
    public void Add_newRow_addsToCollection()
    {
        var testData1 = CreateData("a");
        var testData2 = CreateData("b");
        var sut = new TheoryTestData<ITestData>(testData1, ArgsCode.Instance, null);
        var row = new TheoryTestDataRow<ITestData>(testData2, ArgsCode.Instance, null);
        sut.Add(row);
        Assert.AreEqual(2, sut.Count);
    }

    [TestMethod]
    public void Add_duplicateTestCaseName_ignored()
    {
        var testData1 = CreateData("same");
        var testData2 = CreateData("same");
        var sut = new TheoryTestData<ITestData>(testData1, ArgsCode.Instance, null);
        var row = new TheoryTestDataRow<ITestData>(testData2, ArgsCode.Instance, null);
        sut.Add(row);
        Assert.AreEqual(1, sut.Count);
    }

    [TestMethod]
    public void ArgsCode_instance_isImmutableAfterInit()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, null);
        Assert.AreEqual(ArgsCode.Instance, sut.ArgsCode);
    }

    [TestMethod]
    public void Constructor_firstRow_hasCorrectTestCaseName()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, null);
        var row = sut.Single();
        Assert.AreEqual(testData.TestCaseName, row.TestCaseName);
    }

    [TestMethod]
    public void Constructor_withMethodName_firstRowHasFormattedDisplayName()
    {
        var testData = CreateData("a");
        var sut = new TheoryTestData<ITestData>(testData, ArgsCode.Instance, "MyMethod");
        var row = sut.Single();
        Assert.Contains("MyMethod", row.TestDisplayName!);
        Assert.Contains(testData.TestCaseName, row.TestDisplayName!);
    }

    [TestMethod]
    public void AddRow_newItem_increasesCount()
    {
        var testData1 = CreateData("x");
        var sut = new TheoryTestData<ITestData>(testData1, ArgsCode.Instance, null);
        sut.AddRow(CreateData("y"));
        Assert.AreEqual(2, sut.Count);
    }

    [TestMethod]
    public void AddRow_duplicateTestCaseName_ignored()
    {
        var testData1 = CreateData("dup");
        var sut = new TheoryTestData<ITestData>(testData1, ArgsCode.Instance, null);
        sut.AddRow(CreateData("dup"));
        Assert.AreEqual(1, sut.Count);
    }
}
