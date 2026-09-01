// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;

namespace Tests.Portamical.Core.T4;

[TestClass]
public class GeneratedFactoryBehaviorTests
{
    private const string Definition = "definition";
    private static readonly InvalidOperationException ExpectedException = new("boom");

    [TestMethod]
    public void CreateTestData_arity3_preservesGeneratedArguments()
    {
        var sut = TestDataFactory.CreateTestData<int, string, bool>(
            Definition,
            "result",
            42,
            "hello",
            true);

        Assert.IsInstanceOfType<TestData<int, string, bool>>(sut);
        Assert.AreEqual(42, sut.Arg1);
        Assert.AreEqual("hello", sut.Arg2);
        Assert.IsTrue(sut.Arg3);
    }

    [TestMethod]
    public void CreateTestDataReturns_arity3_toArgsAll_includesTestCaseNameArgumentsAndExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string, bool>(
            Definition,
            5,
            42,
            "hello",
            true);

        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);

        Assert.IsInstanceOfType<TestDataReturns<int, int, string, bool>>(sut);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual(42, args[1]);
        Assert.AreEqual("hello", args[2]);
        Assert.AreEqual(true, args[3]);
        Assert.AreEqual(5, args[4]);
    }

    [TestMethod]
    public void CreateTestDataReturns_arity3_trimReturnsExpected_returnsOnlyArguments()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string, bool>(
            Definition,
            5,
            42,
            "hello",
            true);

        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);

        CollectionAssert.AreEqual(new object?[] { 42, "hello", true }, args);
    }

    [TestMethod]
    public void CreateTestDataThrows_arity3_trimThrowsExpected_returnsOnlyArguments()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int, string, bool>(
            Definition,
            ExpectedException,
            42,
            "hello",
            true);

        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);

        Assert.IsInstanceOfType<TestDataThrows<InvalidOperationException, int, string, bool>>(sut);
        CollectionAssert.AreEqual(new object?[] { 42, "hello", true }, args);
    }
}
