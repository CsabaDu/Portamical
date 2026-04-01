// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Patterns;

namespace Tests.Portamical.NUnit.TestBases.TestCaseDataCollection;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteTestBase : global::Portamical.NUnit.TestBases.TestCaseDataCollection.TestBase
    {
        public static IReadOnlyCollection<global::NUnit.Framework.TestCaseData> InvokeConvert<T>(
            IEnumerable<T> collection, ArgsCode argsCode, string? name = null)
        where T : notnull, ITestData
            => Convert(collection, argsCode, name);

        public static IReadOnlyCollection<global::NUnit.Framework.TestCaseData> InvokeConvertDefault<T>(
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
        Assert.IsInstanceOfType<IReadOnlyCollection<global::NUnit.Framework.TestCaseData>>(result);
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
    public void Convert_returnsNUnitTestCaseDataElements()
    {
        ITestData[] collection = [CreateData("a")];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Instance);
        Assert.IsInstanceOfType<global::NUnit.Framework.TestCaseData>(result.First());
    }

    [TestMethod]
    public void Convert_withArgsCode_argsCodeProperties_argumentsAreFlattened()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = ConcreteTestBase.InvokeConvert(collection, ArgsCode.Properties);
        var expectedArgs = item.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        CollectionAssert.AreEqual(expectedArgs, result.First().Arguments);
    }
}
