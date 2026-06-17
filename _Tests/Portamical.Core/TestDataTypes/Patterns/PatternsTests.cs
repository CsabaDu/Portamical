// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for IExpected, IReturns, IThrows interfaces and their conformance.
// Verifies interface hierarchy, type conformance, GetExpected(), GetResultPrefix(),
// typed Expected property, and covariance.

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Patterns;

namespace Tests.Portamical.Core.TestDataTypes.Patterns;

[TestClass]
public class PatternsTests
{
    private const string Def = "definition";

    #region ITestData conformance
    [TestMethod]
    public void TestData_implements_iTestData()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, "result", 1);
        Assert.IsInstanceOfType<ITestData>(sut);
    }

    [TestMethod]
    public void TestDataReturns_implements_iTestData()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<ITestData>(sut);
    }

    [TestMethod]
    public void TestDataThrows_implements_iTestData()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<ITestData>(sut);
    }
    #endregion

    #region IReturns conformance
    [TestMethod]
    public void TestDataReturns_implements_iReturns()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<IReturns>(sut);
    }

    [TestMethod]
    public void TestDataReturns_implements_iReturns_TStruct()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<IReturns<int>>(sut);
    }

    [TestMethod]
    public void TestDataReturns_implements_iExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns(Def, 5, 1);
        Assert.IsInstanceOfType<IExpected>(sut);
    }

    [TestMethod]
    public void TestDataReturns_implements_iExpected_TStruct()
    {
        var sut = TestDataFactory.CreateTestDataReturns(Def, 5, 1);
        Assert.IsInstanceOfType<IExpected<int>>(sut);
    }

    [TestMethod]
    public void TestDataThrows_doesNotImplement_iReturns()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.IsNotInstanceOfType<IReturns>(sut);
    }
    #endregion

    #region IThrows conformance
    [TestMethod]
    public void TestDataThrows_implements_iThrows()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IThrows>(sut);
    }

    [TestMethod]
    public void TestDataThrows_implements_iThrows_TException()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IThrows<InvalidOperationException>>(sut);
    }

    [TestMethod]
    public void TestDataThrows_implements_iExpected()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IExpected>(sut);
    }

    [TestMethod]
    public void TestDataThrows_implements_iExpected_TException()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IExpected<InvalidOperationException>>(sut);
    }

    [TestMethod]
    public void TestDataReturns_doesNotImplement_iThrows()
    {
        var sut = TestDataFactory.CreateTestDataReturns(Def, 5, 1);
        Assert.IsNotInstanceOfType<IThrows>(sut);
    }
    #endregion

    #region IExpected.GetExpected() — non-generic object access
    [TestMethod]
    public void IExpected_getExpected_returnsExpectedValue_forReturns()
    {
#pragma warning disable CA1859 // Use concrete types when possible - interface type is intentionally used to test non-generic access
        IExpected sut = TestDataFactory.CreateTestDataReturns(Def, 42, 1);
#pragma warning restore CA1859
        Assert.AreEqual(42, sut.GetExpected());
    }

    [TestMethod]
    public void IExpected_getExpected_returnsExpectedInstance_forThrows()
    {
        var ex = new InvalidOperationException();
#pragma warning disable CA1859 // Use concrete types when possible - interface type is intentionally used to test non-generic access
        IExpected sut = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
#pragma warning restore CA1859
        Assert.AreSame(ex, sut.GetExpected());
    }
    #endregion

    #region IExpected.GetResultPrefix()
    [TestMethod]
    public void IExpected_getResultPrefix_returnsReturns_forTestDataReturns()
    {
#pragma warning disable CA1859 // Use concrete types when possible - interface type is intentionally used to test non-generic access
        IExpected sut = TestDataFactory.CreateTestDataReturns(Def, 5, 1);
#pragma warning restore CA1859
        Assert.AreEqual("returns", sut.GetResultPrefix());
    }

    [TestMethod]
    public void IExpected_getResultPrefix_returnsThrows_forTestDataThrows()
    {
#pragma warning disable CA1859 // Use concrete types when possible - interface type is intentionally used to test non-generic access
        IExpected sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
#pragma warning restore CA1859
        Assert.AreEqual("throws", sut.GetResultPrefix());
    }
    #endregion

    #region IExpected<T>.Expected — typed property access
    [TestMethod]
    public void IExpectedT_expected_hasExpectedValue_forReturns()
    {
#pragma warning disable CA1859 // Use concrete types when possible - interface type is intentionally used to test non-generic access
        IExpected<int> sut = TestDataFactory.CreateTestDataReturns(Def, 42, 1);
#pragma warning restore CA1859
        Assert.AreEqual(42, sut.Expected);
    }

    [TestMethod]
    public void IExpectedT_expected_hasExpectedInstance_forThrows()
    {
        var ex = new InvalidOperationException();
#pragma warning disable CA1859 // Use concrete types when possible - interface type is intentionally used to test non-generic access
        IExpected<InvalidOperationException> sut = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
        var actual = sut.Expected;
        Assert.AreSame(ex, actual);
    }
    #endregion

    #region Covariance
    [TestMethod]
    public void IThrows_isAssignableTo_IThrowsBaseException_viaCovariance()
    {
        var ex = new ArgumentException("ignore test message");
        IThrows<ArgumentException> derived = TestDataFactory.CreateTestDataThrows(Def, ex, 1);
        IThrows<Exception> covariant = derived;    // IThrows<ArgumentException> → IThrows<Exception>
        Assert.IsNotNull(covariant);
    }
    #endregion
}
