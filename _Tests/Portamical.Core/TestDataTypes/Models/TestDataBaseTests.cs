// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for TestDataBase public API:
// GetDefinition(), ToArgs(ArgsCode) single-arg overload, and invalid-enum validation.

using System.ComponentModel;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.TestDataTypes.Models;

[TestClass]
public class TestDataBaseTests
{
    private const string Def = "scenario definition";
    private const string Result = "result";

    #region GetDefinition
    [TestMethod]
    public void GetDefinition_returnsDefinitionString()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 1);
        Assert.AreEqual(Def, sut.GetDefinition());
    }

    [TestMethod]
    public void GetDefinition_forTestDataReturns_returnsDefinitionString()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual(Def, sut.GetDefinition());
    }

    [TestMethod]
    public void GetDefinition_forTestDataThrows_returnsDefinitionString()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual(Def, sut.GetDefinition());
    }
    #endregion

    #region ToArgs single-arg overload (uses PropsCode.TrimTestCaseName by default)
    [TestMethod]
    public void ToArgs_instance_singleArgOverload_equals_twoArgVersion_withAnyPropsCode()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var oneArg = sut.ToArgs(ArgsCode.Instance);
        var twoArg = sut.ToArgs(ArgsCode.Instance, PropsCode.All);
        // Instance mode ignores PropsCode — both should return [sut]
        Assert.HasCount(1, oneArg);
        Assert.AreSame(sut, oneArg[0]);
        Assert.HasCount(1, twoArg);
        Assert.AreSame(sut, twoArg[0]);
    }

    [TestMethod]
    public void ToArgs_properties_singleArgOverload_usesTrimTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var oneArg = sut.ToArgs(ArgsCode.Properties);
        var twoArg = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        // Both should return [Arg1] with TestCaseName trimmed
        Assert.HasCount(1, oneArg);
        Assert.AreEqual(42, oneArg[0]);
        Assert.HasCount(1, twoArg);
        Assert.AreEqual(42, twoArg[0]);
    }

    [TestMethod]
    public void ToArgs_properties_singleArgOverload_forReturns_usesTrimTestCaseName()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var oneArg = sut.ToArgs(ArgsCode.Properties);
        var twoArg = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        // Both should return [Expected=5, Arg1=42]
        Assert.HasCount(2, oneArg);
        Assert.AreEqual(5, oneArg[0]);
        Assert.AreEqual(42, oneArg[1]);
        Assert.HasCount(2, twoArg);
        Assert.AreEqual(5, twoArg[0]);
        Assert.AreEqual(42, twoArg[1]);
    }
    #endregion

    #region Invalid enum argument validation
    [TestMethod]
    public void ToArgs_properties_withUndefinedPropsCode_throwsInvalidEnumArgumentException()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 1);
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => sut.ToArgs(ArgsCode.Properties, (PropsCode)99));
    }

    [TestMethod]
    public void ToArgs_withUndefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 1);
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => sut.ToArgs((ArgsCode)99, PropsCode.All));
    }
    #endregion
}
