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
    public void testData_implements_iTestData()
    {
        var sut = TestDataFactory.CreateTestData<int>(Def, "result", 1);
        Assert.IsInstanceOfType<ITestData>(sut);
    }

    [TestMethod]
    public void testDataReturns_implements_iTestData()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<ITestData>(sut);
    }

    [TestMethod]
    public void testDataThrows_implements_iTestData()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<ITestData>(sut);
    }
    #endregion

    #region IReturns conformance
    [TestMethod]
    public void testDataReturns_implements_iReturns()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<IReturns>(sut);
    }

    [TestMethod]
    public void testDataReturns_implements_iReturns_TStruct()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<IReturns<int>>(sut);
    }

    [TestMethod]
    public void testDataReturns_implements_iExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<IExpected>(sut);
    }

    [TestMethod]
    public void testDataReturns_implements_iExpected_TStruct()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsInstanceOfType<IExpected<int>>(sut);
    }

    [TestMethod]
    public void testDataThrows_doesNotImplement_iReturns()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.IsNotInstanceOfType<IReturns>(sut);
    }
    #endregion

    #region IThrows conformance
    [TestMethod]
    public void testDataThrows_implements_iThrows()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IThrows>(sut);
    }

    [TestMethod]
    public void testDataThrows_implements_iThrows_TException()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IThrows<InvalidOperationException>>(sut);
    }

    [TestMethod]
    public void testDataThrows_implements_iExpected()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IExpected>(sut);
    }

    [TestMethod]
    public void testDataThrows_implements_iExpected_TException()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.IsInstanceOfType<IExpected<InvalidOperationException>>(sut);
    }

    [TestMethod]
    public void testDataReturns_doesNotImplement_iThrows()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.IsNotInstanceOfType<IThrows>(sut);
    }
    #endregion

    #region IExpected.GetExpected() — non-generic object access
    [TestMethod]
    public void iExpected_getExpected_returnsExpectedValue_forReturns()
    {
        IExpected sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 42, 1);
        Assert.AreEqual(42, sut.GetExpected());
    }

    [TestMethod]
    public void iExpected_getExpected_returnsExpectedInstance_forThrows()
    {
        var ex = new InvalidOperationException();
        IExpected sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 1);
        Assert.AreSame(ex, sut.GetExpected());
    }
    #endregion

    #region IExpected.GetResultPrefix()
    [TestMethod]
    public void iExpected_getResultPrefix_returnsReturns_forTestDataReturns()
    {
        IExpected sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual("returns", sut.GetResultPrefix());
    }

    [TestMethod]
    public void iExpected_getResultPrefix_returnsThrows_forTestDataThrows()
    {
        IExpected sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual("throws", sut.GetResultPrefix());
    }
    #endregion

    #region IExpected<T>.Expected — typed property access
    [TestMethod]
    public void iExpectedT_expected_hasExpectedValue_forReturns()
    {
        IExpected<int> sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 42, 1);
        Assert.AreEqual(42, sut.Expected);
    }

    [TestMethod]
    public void iExpectedT_expected_hasExpectedInstance_forThrows()
    {
        var ex = new InvalidOperationException();
        IExpected<InvalidOperationException> sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 1);
        var actual = sut.Expected;
        Assert.AreSame(ex, actual);
    }
    #endregion

    #region Covariance
    [TestMethod]
    public void iThrows_isAssignableTo_iThrowsBaseException_viaCovariance()
    {
        IThrows<ArgumentException> derived = TestDataFactory.CreateTestDataThrows<ArgumentException, int>(Def, new ArgumentException(), 1);
        IThrows<Exception> covariant = derived;    // IThrows<ArgumentException> → IThrows<Exception>
        Assert.IsNotNull(covariant);
    }
    #endregion
}
