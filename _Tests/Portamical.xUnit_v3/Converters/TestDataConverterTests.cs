// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Tests.Portamical.xUnit_v3.Converters;

[TestClass]
public class TestDataConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    // ToTheoryTestData

    [TestMethod]
    public void ToTheoryTestData_returnsCorrectType()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestData(ArgsCode.Instance, null);
        Assert.IsInstanceOfType<TheoryTestData<ITestData>>(result);
    }

    [TestMethod]
    public void ToTheoryTestData_setsArgsCode()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestData(ArgsCode.Properties, null);
        Assert.AreEqual(ArgsCode.Properties, result.ArgsCode);
    }

    [TestMethod]
    public void ToTheoryTestData_setsTestMethodName()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestData(ArgsCode.Instance, "TestMethod");
        Assert.AreEqual("TestMethod", result.TestMethodName);
    }

    [TestMethod]
    public void ToTheoryTestData_nullTestMethodName_testMethodNameIsNull()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestData(ArgsCode.Instance, null);
        Assert.IsNull(result.TestMethodName);
    }

    [TestMethod]
    public void ToTheoryTestData_addsFirstRowOnConstruction()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestData(ArgsCode.Instance, null);
        Assert.AreEqual(1, result.Count);
    }

    // ToTheoryTestDataRow (generic)

    [TestMethod]
    public void ToTheoryTestDataRow_generic_returnsCorrectType()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance, null);
        Assert.IsInstanceOfType<TheoryTestDataRow<ITestData>>(result);
    }

    [TestMethod]
    public void ToTheoryTestDataRow_generic_setsTestCaseName()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance, null);
        Assert.AreEqual(testData.TestCaseName, result.TestCaseName);
    }

    [TestMethod]
    public void ToTheoryTestDataRow_generic_withMethodName_setsDisplayName()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance, "TestMethod");
        Assert.Contains("TestMethod", result.TestDisplayName!);
        Assert.Contains(testData.TestCaseName, result.TestDisplayName!);
    }

    [TestMethod]
    public void ToTheoryTestDataRow_generic_nullMethodName_displayNameIsNull()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance, null);
        Assert.IsNull(result.TestDisplayName);
    }

    // ToTheoryTestDataRow (non-generic)

    [TestMethod]
    public void ToTheoryTestDataRow_nonGeneric_returnsTheoryTestDataRow()
    {
        ITestData testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance);
        Assert.IsInstanceOfType<TheoryTestDataRow>(result);
    }

    [TestMethod]
    public void ToTheoryTestDataRow_nonGeneric_setsTestCaseName()
    {
        ITestData testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance);
        Assert.AreEqual(testData.TestCaseName, result.TestCaseName);
    }

    [TestMethod]
    public void ToTheoryTestDataRow_nonGeneric_displayNameIsNull()
    {
        ITestData testData = CreateData("a");
        var result = testData.ToTheoryTestDataRow(ArgsCode.Instance);
        Assert.IsNull(result.TestDisplayName);
    }

    // ToTheoryDataRow

    [TestMethod]
    public void ToTheoryDataRow_returnsITheoryDataRow()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryDataRow(ArgsCode.Instance, null);
        Assert.IsInstanceOfType<Xunit.ITheoryDataRow>(result);
    }

    [TestMethod]
    public void ToTheoryDataRow_instance_getDataWrapsTestData()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryDataRow(ArgsCode.Instance, null);
        var data = result.GetData();
        Assert.HasCount(1, data);
        Assert.AreSame(testData, data[0]);
    }

    [TestMethod]
    public void ToTheoryDataRow_properties_getDataWrapsArgsArray()
    {
        var testData = CreateData("a");
        var result = testData.ToTheoryDataRow(ArgsCode.Properties, null);
        var data = result.GetData();
        // TheoryDataRow(object) wraps the args array as a single element
        Assert.HasCount(1, data);
        Assert.IsInstanceOfType<object?[]>(data[0]);
        var innerArgs = (object?[])data[0]!;
        var sut_expected = testData.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(sut_expected, innerArgs);
    }
}
