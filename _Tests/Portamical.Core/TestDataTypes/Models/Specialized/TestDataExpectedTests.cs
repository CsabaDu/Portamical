// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for TestDataExpected<TResult>, TestDataReturns<TStruct>, and TestDataThrows<TException>
// base class behaviour: GetExpected(), GetResultPrefix(), and family-specific trimming.

using Portamical.Core.Factories;
using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.TestDataTypes.Models.Specialized;

[TestClass]
public class TestDataExpectedTests
{
    private const string Def = "definition";

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
}
