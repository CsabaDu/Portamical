// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for the TestData abstract class (general-purpose test data with custom result strings).
// Focuses on trimming behaviour specific to TestData: any non-All PropsCode removes TestCaseName.

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

    [TestMethod]
    public void toArgs_properties_trimReturnsExpected_removesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }

    [TestMethod]
    public void toArgs_properties_trimThrowsExpected_removesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }

    [TestMethod]
    public void toArgs_properties_all_includesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(2, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual(42, args[1]);
    }
}
