// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.ComponentModel;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit.Converters;
using Portamical.xUnit.DataProviders;

namespace Tests.Portamical.xUnit.Converters;

[TestClass]
public class TestDataConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void InitTestDataProvider_validArgsCode_returnsTestDataProvider()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.InitTestDataProvider(testData, ArgsCode.Instance);
        Assert.IsInstanceOfType<TestDataProvider<ITestData>>(result);
    }

    [TestMethod]
    public void InitTestDataProvider_setsArgsCode()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.InitTestDataProvider(testData, ArgsCode.Properties);
        Assert.AreEqual(ArgsCode.Properties, result.ArgsCode);
    }

    [TestMethod]
    public void InitTestDataProvider_testMethodName_alwaysNull()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.InitTestDataProvider(testData, ArgsCode.Instance, "SomeName");
        Assert.IsNull(result.TestMethodName);
    }

    [TestMethod]
    public void InitTestDataProvider_nullTestMethodName_testMethodNameIsNull()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.InitTestDataProvider(testData, ArgsCode.Instance, null);
        Assert.IsNull(result.TestMethodName);
    }

    [TestMethod]
    public void InitTestDataProvider_addsFirstRowOnConstruction()
    {
        var testData = CreateData("a");
        var result = TestDataConverter.InitTestDataProvider(testData, ArgsCode.Instance);
        var rows = result.Cast<object?[]>().ToList();
        Assert.HasCount(1, rows);
        Assert.AreSame(testData, rows[0][0]);
    }

    [TestMethod]
    public void InitTestDataProvider_invalidArgsCode_throwsInvalidEnumArgumentException()
    {
        var testData = CreateData("a");
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => TestDataConverter.InitTestDataProvider(testData, (ArgsCode)99));
    }
}
