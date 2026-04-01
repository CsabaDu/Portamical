// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.NUnit.TestDataTypes;

namespace Tests.Portamical.NUnit.TestBases;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteTestBase : global::Portamical.NUnit.TestBases.TestBase
    {
        public static IReadOnlyCollection<TestCaseTestData> InvokeConvert<T>(
            IEnumerable<T> collection, ArgsCode argsCode, string? name = null)
        where T : notnull, ITestData
            => Convert(collection, argsCode, name);

        public static IReadOnlyCollection<TestCaseTestData> InvokeConvertDefault<T>(
            IEnumerable<T> collection, string? name = null)
        where T : notnull, ITestData
            => Convert(collection, name);
    }

    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void Convert_withArgsCode_returnsIReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<TestCaseTestData>>(result);
    }

    [TestMethod]
    public void Convert_withArgsCode_hasCorrectCount()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void Convert_withArgsCode_deduplicatesByTestCaseName()
    {
        var dup = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), dup];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Convert_withArgsCode_andMethodName_setsTestName()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance, "TestMethod");
        Assert.StartsWith("TestMethod", result.First().TestName!);
    }

    [TestMethod]
    public void Convert_withArgsCode_argsCodeInstance_argumentIsTestData()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.AreSame(item, result.First().Arguments![0]);
    }

    [TestMethod]
    public void Convert_asInstance_usesInstanceArgsCode()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.InvokeConvertDefault(collection);
        Assert.AreSame(item, result.First().Arguments![0]);
    }

    [TestMethod]
    public void Convert_asInstance_hasCorrectCount()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b")];
        var result = ConcreteTestBase.InvokeConvertDefault(collection);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void Convert_asInstance_withMethodName_setsTestName()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.InvokeConvertDefault(collection, "TestMethod");
        Assert.StartsWith("TestMethod", result.First().TestName!);
    }

    [TestMethod]
    public void Convert_returnsTestCaseTestDataElements()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.IsInstanceOfType<TestCaseTestData>(result.First());
    }
}
