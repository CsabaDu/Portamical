// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for TestDataFactory (TestDataFactory.generated.cs).
// Covers factory-specific contracts not exercised by the T4 template contract tests:
//   - Null arguments pass through to ArgN properties
//   - Each call returns a new independent instance (no caching)
//   - Non-int generic types work correctly (bool struct, string reference, exception subtypes)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;

namespace Tests.Portamical.Core.Factories;

[TestClass]
public class TestDataFactoryTests
{
    private const string Def = "definition";
    private const string Result = "result";

    #region CreateTestData — null argument handling
    [TestMethod]
    public void CreateTestData_withNullReferenceArg1_setsArg1ToNull()
    {
        var sut = TestDataFactory.CreateTestData<string>(Def, Result, null);
        Assert.IsNull(sut.Arg1);
    }

    [TestMethod]
    public void CreateTestData_withNullArg1_andNonNullArg2_setsPropertiesCorrectly()
    {
        var sut = TestDataFactory.CreateTestData<string, int>(Def, Result, null, 99);
        Assert.IsNull(sut.Arg1);
        Assert.AreEqual(99, sut.Arg2);
    }
    #endregion

    #region CreateTestData — independent instances (no caching)
    [TestMethod]
    public void CreateTestData_calledTwice_withSameArgs_returnsDifferentInstances()
    {
        var a = TestDataFactory.CreateTestData<int>(Def, Result, 1);
        var b = TestDataFactory.CreateTestData<int>(Def, Result, 1);
        Assert.AreNotSame(a, b);
    }
    #endregion

    #region CreateTestData — non-int generic types
    [TestMethod]
    public void CreateTestData_withBoolArg_setsArg1Correctly()
    {
        var sut = TestDataFactory.CreateTestData<bool>(Def, Result, true);
        Assert.IsInstanceOfType<TestData<bool>>(sut);
        Assert.IsTrue(sut.Arg1);
    }

    [TestMethod]
    public void CreateTestData_withDoubleArg_setsArg1Correctly()
    {
        var sut = TestDataFactory.CreateTestData<double>(Def, Result, 3.14);
        Assert.IsInstanceOfType<TestData<double>>(sut);
        Assert.AreEqual(3.14, sut.Arg1);
    }
    #endregion

    #region CreateTestDataReturns — null argument handling
    [TestMethod]
    public void CreateTestDataReturns_withNullReferenceArg1_setsArg1ToNull()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, string>(Def, 5, null);
        Assert.IsNull(sut.Arg1);
    }
    #endregion

    #region CreateTestDataReturns — independent instances
    [TestMethod]
    public void CreateTestDataReturns_calledTwice_withSameArgs_returnsDifferentInstances()
    {
        var a = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        var b = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreNotSame(a, b);
    }
    #endregion

    #region CreateTestDataReturns — non-int struct types
    [TestMethod]
    public void CreateTestDataReturns_withBoolExpected_setsExpectedAndTestCaseName()
    {
        var sut = TestDataFactory.CreateTestDataReturns<bool, int>(Def, true, 1);
        Assert.IsInstanceOfType<TestDataReturns<bool, int>>(sut);
        Assert.IsTrue(sut.Expected);
        Assert.AreEqual($"{Def} => returns True", sut.TestCaseName);
    }

    [TestMethod]
    public void CreateTestDataReturns_withDoubleExpected_formatsTestCaseName()
    {
        var sut = TestDataFactory.CreateTestDataReturns<double, int>(Def, 3.14, 1);
        Assert.AreEqual(3.14, sut.Expected);
        Assert.AreEqual($"{Def} => returns {3.14}", sut.TestCaseName);
    }
    #endregion

    #region CreateTestDataThrows — null argument handling
    [TestMethod]
    public void CreateTestDataThrows_withNullReferenceArg1_setsArg1ToNull()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, string>(Def, new InvalidOperationException(), null);
        Assert.IsNull(sut.Arg1);
    }
    #endregion

    #region CreateTestDataThrows — independent instances
    [TestMethod]
    public void CreateTestDataThrows_calledTwice_withSameArgs_returnsDifferentInstances()
    {
        var ex = new InvalidOperationException();
        var a = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
        var b = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
        Assert.AreNotSame(a, b);
    }
    #endregion

    #region CreateTestDataThrows — exception subtypes
    [TestMethod]
    public void CreateTestDataThrows_withArgumentException_setsExpectedAndTestCaseName()
    {
        var ex = new ArgumentException("bad value");
        var sut = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
        Assert.IsInstanceOfType<TestDataThrows<ArgumentException, int>>(sut);
        Assert.AreSame(sut.Expected, ex);
        Assert.AreEqual($"{Def} => throws ArgumentException", sut.TestCaseName);
    }

    [TestMethod]
    public void CreateTestDataThrows_withArgumentNullException_usesConcreteTypeName()
    {
        // ArgumentNullException is a subtype of ArgumentException — type name used, not base
        var paramName = "param";
        var ex = new ArgumentNullException(paramName);
        var sut = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
        Assert.AreEqual($"{Def} => throws ArgumentNullException", sut.TestCaseName);
    }
    #endregion
}
