// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for TestDataExpected<TResult>, TestDataReturns<TStruct>, and TestDataThrows<TException>
// base class behaviour: GetExpected(), GetResultPrefix(), GetResult(), TestCaseName format,
// ToArgs with all PropsCode combinations, and family-specific trimming.

using Portamical.Core.Factories;
using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.TestDataTypes.Models.Specialized;

[TestClass]
public class TestDataExpectedTests
{
    private const string Def = "definition";

    #region GetResult()
    [TestMethod]
    public void testDataReturns_getResult_hasFormat_returnsExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual("returns 5", sut.GetResult());
    }

    [TestMethod]
    public void testDataThrows_getResult_hasFormat_throwsExceptionTypeName()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual($"throws {nameof(InvalidOperationException)}", sut.GetResult());
    }
    #endregion

    #region TestCaseName
    [TestMethod]
    public void testDataReturns_testCaseName_hasFormat_definitionArrowReturnsExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual($"{Def} => returns 5", sut.TestCaseName);
    }

    [TestMethod]
    public void testDataThrows_testCaseName_hasFormat_definitionArrowThrowsTypeName()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual($"{Def} => throws {nameof(InvalidOperationException)}", sut.TestCaseName);
    }
    #endregion

    #region GetExpected() — non-generic polymorphic access
    [TestMethod]
    public void getExpected_returnsExpected_asObject_forReturns()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 42, 1);
        Assert.AreEqual(42, sut.GetExpected());
    }

    [TestMethod]
    public void getExpected_returnsExpected_asObject_forThrows()
    {
        var ex = new InvalidOperationException();
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 1);
        Assert.AreSame(ex, sut.GetExpected());
    }
    #endregion

    #region GetResultPrefix()
    [TestMethod]
    public void getResultPrefix_returnsReturns_forTestDataReturns()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual("returns", sut.GetResultPrefix());
    }

    [TestMethod]
    public void getResultPrefix_returnsThrows_forTestDataThrows()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual("throws", sut.GetResultPrefix());
    }
    #endregion

    #region TestDataReturns — TrimThrowsExpected behaves as TrimTestCaseName
    [TestMethod]
    public void testDataReturns_toArgs_trimThrowsExpected_leavesExpected_removesTestCaseName()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
        // TrimThrowsExpected is not this family's code: removes TestCaseName but keeps Expected
        Assert.HasCount(2, args);
        Assert.AreEqual(5, args[0]);    // Expected
        Assert.AreEqual(42, args[1]);   // Arg1
    }
    #endregion

    #region TestDataReturns — ToArgs with PropsCode
    [TestMethod]
    public void testDataReturns_toArgs_properties_all_returnsTestCaseName_expected_andArg1()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(3, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual(5, args[1]);
        Assert.AreEqual(42, args[2]);
    }

    [TestMethod]
    public void testDataReturns_toArgs_properties_trimTestCaseName_returnsExpected_andArg1()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        Assert.HasCount(2, args);
        Assert.AreEqual(5, args[0]);
        Assert.AreEqual(42, args[1]);
    }

    [TestMethod]
    public void testDataReturns_toArgs_properties_trimReturnsExpected_removesTestCaseName_andExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }
    #endregion

    #region TestDataThrows — TrimReturnsExpected behaves as TrimTestCaseName
    [TestMethod]
    public void testDataThrows_toArgs_trimReturnsExpected_leavesExpected_removesTestCaseName()
    {
        var ex = new InvalidOperationException();
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        // TrimReturnsExpected is not this family's code: removes TestCaseName but keeps Expected
        Assert.HasCount(2, args);
        Assert.AreSame(ex, args[0]);   // Expected (exception instance)
        Assert.AreEqual(42, args[1]);  // Arg1
    }
    #endregion

    #region TestDataThrows — ToArgs with PropsCode
    [TestMethod]
    public void testDataThrows_toArgs_properties_all_returnsTestCaseName_expected_andArg1()
    {
        var ex = new InvalidOperationException();
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(3, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreSame(ex, args[1]);
        Assert.AreEqual(42, args[2]);
    }

    [TestMethod]
    public void testDataThrows_toArgs_properties_trimTestCaseName_returnsExpected_andArg1()
    {
        var ex = new InvalidOperationException();
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        Assert.HasCount(2, args);
        Assert.AreSame(ex, args[0]);
        Assert.AreEqual(42, args[1]);
    }

    [TestMethod]
    public void testDataThrows_toArgs_properties_trimThrowsExpected_removesTestCaseName_andExpected()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }
    #endregion
}
