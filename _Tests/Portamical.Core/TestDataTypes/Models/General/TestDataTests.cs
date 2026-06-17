// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for the TestData abstract class (general-purpose test data with custom result strings).
// Covers TestCaseName format, GetResult, and PropsCode-specific trimming for TestData.

using Portamical.Core.Factories;
using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.TestDataTypes.Models.General;

[TestClass]
public class TestDataTests
{
    private const string Def = "definition";
    private const string Result = "result";

    // TestData.ToArgs trims TestCaseName for ANY non-All PropsCode.
    // This verifies that TrimReturnsExpected and TrimThrowsExpected behave
    // the same as TrimTestCaseName when applied to TestData (not TestDataReturns/Throws).

    #region TestCaseName and GetResult
    [TestMethod]
    public void TestCaseName_hasFormat_definitionArrowResult()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 1);
        Assert.AreEqual($"{Def} => {Result}", sut.TestCaseName);
    }

    [TestMethod]
    public void GetResult_returnsResultString()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        Assert.AreEqual(Result, sut.GetResult());
    }
    #endregion

    #region ToArgs — PropsCode trimming
    [TestMethod]
    public void ToArgs_properties_trimReturnsExpected_removesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }

    [TestMethod]
    public void ToArgs_properties_trimThrowsExpected_removesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }

    [TestMethod]
    public void ToArgs_properties_all_includesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(2, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual(42, args[1]);
    }

    [TestMethod]
    public void ToArgs_properties_all_arity2_returnsTestCaseName_andBothArgs()
    {
        var sut = TestDataFactory.CreateTestData<int, string>(Def, Result, 42, "hello");
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(3, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual(42, args[1]);
        Assert.AreEqual("hello", args[2]);
    }
    #endregion
}
