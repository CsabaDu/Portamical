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
public class TestDataConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void ToTestCaseTestData_returnsCorrectType()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseTestData(testData, ArgsCode.Instance);
        Assert.IsInstanceOfType<TestCaseTestData<ITestData>>(result);
    }

    [TestMethod]
    public void ToTestCaseTestData_setsTestCaseName()
    {
        var testData = CreateData("myCaseName");
        var result = TestDataConverter.ToTestCaseTestData(testData, ArgsCode.Instance);
        Assert.AreEqual(testData.TestCaseName, result.TestCaseName);
    }

    [TestMethod]
    public void ToTestCaseTestData_withMethodName_setsTestName()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseTestData(testData, ArgsCode.Instance, "TestMethod");
        Assert.IsNotNull(result.TestName);
        Assert.StartsWith("TestMethod", result.TestName!);
    }

    [TestMethod]
    public void ToTestCaseTestData_withoutMethodName_testNameIsTestCaseName()
    {
        var testData = CreateData("myCaseName");
        var result = TestDataConverter.ToTestCaseTestData(testData, ArgsCode.Instance, null);
        Assert.AreEqual(testData.TestCaseName, result.TestName);
    }

    [TestMethod]
    public void ToTestCaseTestData_argsCodeInstance_argumentIsTestData()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseTestData(testData, ArgsCode.Instance);
        Assert.AreSame(testData, result.Arguments![0]);
    }

    [TestMethod]
    public void ToTestCaseTestData_argsCodeProperties_argumentsAreFlattened()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseTestData(testData, ArgsCode.Properties);
        var expectedArgs = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        CollectionAssert.AreEqual(expectedArgs, result.Arguments);
    }

    [TestMethod]
    public void ToTestCaseData_returnsTestCaseDataType()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseData(testData, ArgsCode.Instance, null);
        Assert.IsInstanceOfType<global::NUnit.Framework.TestCaseData>(result);
    }

    [TestMethod]
    public void ToTestCaseData_setsTestCaseName()
    {
        var testData = CreateData("myCaseName");
        var result = TestDataConverter.ToTestCaseData(testData, ArgsCode.Instance, null);
        var namedCase = (TestCaseTestData)result;
        Assert.AreEqual(testData.TestCaseName, namedCase.TestCaseName);
    }

    [TestMethod]
    public void ToTestCaseData_runtimeTypeIsTestCaseTestData()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseData(testData, ArgsCode.Instance, null);
        Assert.IsInstanceOfType<TestCaseTestData>(result);
    }

    [TestMethod]
    public void ToTestCaseData_withMethodName_setsTestName()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.ToTestCaseData(testData, ArgsCode.Instance, "TestMethod");
        Assert.IsNotNull(result.TestName);
        Assert.StartsWith("TestMethod", result.TestName!);
    }
}
