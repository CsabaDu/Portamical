// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for TestDataExpected<TResult>, TestDataReturns<TStruct>, and TestDataThrows<TException>
// base class behaviour: GetExpected(), GetResultPrefix(), GetResult(), TestCaseName format,
// ToArgs with all PropsCode combinations, and family-specific trimming.

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Models.Specialized;

namespace Tests.Portamical.Core.TestDataTypes.Models.Specialized;

[TestClass]
public class TestDataExpectedTests
{
    private const string Def = "definition";

    #region Test Helper Classes
    private sealed class TestDataExpectedString(string definition, string expected, string? arg1 = null) : TestDataExpected<string>(definition, expected)
    {
        public string? Arg1 { get; init; } = arg1;

        public override string GetResult()
        => GetExpectedResult(Expected);

        public override string GetResultPrefix()
        => GetValidResultPrefix("results");

        protected override object?[] ToObjectArray(ArgsCode argsCode)
        => Extend(base.ToObjectArray, argsCode, Arg1);
    }

    private sealed class TestDataExpectedInt(string definition, int expected, int arg1 = 0) : TestDataExpected<int>(definition, expected)
    {
        public int Arg1 { get; init; } = arg1;

        public override string GetResult()
        => GetExpectedResult(Expected.ToString());

        public override string GetResultPrefix()
        => GetValidResultPrefix("results");

        protected override object?[] ToObjectArray(ArgsCode argsCode)
        => Extend(base.ToObjectArray, argsCode, Arg1);
    }
    #endregion

    #region GetResult() - TestDataExpected
    [TestMethod]
    public void TestDataExpected_getResult_withString_hasFormat_resultsExpectedValue()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        Assert.AreEqual("results hello", sut.GetResult());
    }

    [TestMethod]
    public void TestDataExpected_getResult_withInt_hasFormat_resultsExpectedValue()
    {
        var sut = new TestDataExpectedInt(Def, 42, 1);
        Assert.AreEqual("results 42", sut.GetResult());
    }
    #endregion

    #region GetResult()
    [TestMethod]
    public void TestDataReturns_getResult_hasFormat_returnsExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual("returns 5", sut.GetResult());
    }

    [TestMethod]
    public void TestDataThrows_getResult_hasFormat_throwsExceptionTypeName()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual($"throws {nameof(InvalidOperationException)}", sut.GetResult());
    }
    #endregion

    #region TestCaseName - TestDataExpected
    [TestMethod]
    public void TestDataExpected_testCaseName_hasFormat_definitionArrowResultsExpected()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        Assert.AreEqual($"{Def} => results hello", sut.TestCaseName);
    }
    #endregion

    #region TestCaseName
    [TestMethod]
    public void TestDataReturns_testCaseName_hasFormat_definitionArrowReturnsExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual($"{Def} => returns 5", sut.TestCaseName);
    }

    [TestMethod]
    public void TestDataThrows_testCaseName_hasFormat_definitionArrowThrowsTypeName()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual($"{Def} => throws {nameof(InvalidOperationException)}", sut.TestCaseName);
    }
    #endregion

    #region GetExpected() — non-generic polymorphic access
    [TestMethod]
    public void TestDataExpected_getExpected_returnsExpected_asObject_forTestDataExpected()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        Assert.AreEqual("hello", sut.GetExpected());
    }

    [TestMethod]
    public void TestDataReturns_getExpected_returnsExpected_asObject_forReturns()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 42, 1);
        Assert.AreEqual(42, sut.GetExpected());
    }

    [TestMethod]
    public void TestDataThrows_getExpected_returnsExpected_asObject_forThrows()
    {
        var ex = new InvalidOperationException();
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 1);
        Assert.AreSame(ex, sut.GetExpected());
    }
    #endregion

    #region GetResultPrefix() - TestDataExpected
    [TestMethod]
    public void TestDataExpected_getResultPrefix_returnsResults()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        Assert.AreEqual("results", sut.GetResultPrefix());
    }
    #endregion

    #region GetResultPrefix()
    [TestMethod]
    public void TestDataReturns_getResultPrefix_returnsReturns_forTestDataReturns()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 1);
        Assert.AreEqual("returns", sut.GetResultPrefix());
    }

    [TestMethod]
    public void TestDataThrows_getResultPrefix_returnsThrows_forTestDataThrows()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 1);
        Assert.AreEqual("throws", sut.GetResultPrefix());
    }
    #endregion


    #region TestDataExpected — ToArgs with PropsCode
    [TestMethod]
    public void TestDataExpected_toArgs_properties_all_returnsTestCaseName_expected_andArg1()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(3, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual("hello", args[1]);
        Assert.AreEqual("input", args[2]);
    }

    [TestMethod]
    public void TestDataExpected_toArgs_properties_trimTestCaseName_removesTestCaseName_removesExpected()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        // TrimTestCaseName removes TestCaseName but keeps Expected
        Assert.HasCount(2, args);
        Assert.AreEqual("hello", args[0]);  // Expected
        Assert.AreEqual("input", args[1]);  // Arg1
    }

    [TestMethod]
    public void TestDataExpected_toArgs_properties_trimReturnsExpected_removesTestCaseName_removesExpected()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        // TrimReturnsExpected is not this family's code: removes TestCaseName but keeps Expected
        Assert.HasCount(2, args);
        Assert.AreEqual("hello", args[0]);  // Expected
        Assert.AreEqual("input", args[1]);  // Arg1
    }

    [TestMethod]
    public void TestDataExpected_toArgs_properties_trimThrowsExpected_removesTestCaseName_removesExpected()
    {
        var sut = new TestDataExpectedString(Def, "hello", "input");
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
        // TrimThrowsExpected is not this family's code: removes TestCaseName but keeps Expected
        Assert.HasCount(2, args);
        Assert.AreEqual("hello", args[0]);  // Expected
        Assert.AreEqual("input", args[1]);  // Arg1
    }
    #endregion


    #region TestDataReturns — TrimThrowsExpected behaves as TrimTestCaseName
    [TestMethod]
    public void TestDataReturns_toArgs_trimThrowsExpected_leavesExpected_removesTestCaseName()
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
    public void TestDataReturns_toArgs_properties_all_returnsTestCaseName_expected_andArg1()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(3, args);
        Assert.AreEqual(sut.TestCaseName, args[0]);
        Assert.AreEqual(5, args[1]);
        Assert.AreEqual(42, args[2]);
    }

    [TestMethod]
    public void TestDataReturns_toArgs_properties_trimTestCaseName_returnsExpected_andArg1()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        Assert.HasCount(2, args);
        Assert.AreEqual(5, args[0]);
        Assert.AreEqual(42, args[1]);
    }

    [TestMethod]
    public void TestDataReturns_toArgs_properties_trimReturnsExpected_removesTestCaseName_andExpected()
    {
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 5, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }
    #endregion

    #region TestDataThrows — TrimReturnsExpected behaves as TrimTestCaseName
    [TestMethod]
    public void TestDataThrows_toArgs_trimReturnsExpected_leavesExpected_removesTestCaseName()
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
    public void TestDataThrows_toArgs_properties_all_returnsTestCaseName_expected_andArg1()
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
    public void TestDataThrows_toArgs_properties_trimTestCaseName_returnsExpected_andArg1()
    {
        var ex = new InvalidOperationException();
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, ex, 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        Assert.HasCount(2, args);
        Assert.AreSame(ex, args[0]);
        Assert.AreEqual(42, args[1]);
    }

    [TestMethod]
    public void TestDataThrows_toArgs_properties_trimThrowsExpected_removesTestCaseName_andExpected()
    {
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int>(Def, new InvalidOperationException(), 42);
        var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
        Assert.HasCount(1, args);
        Assert.AreEqual(42, args[0]);
    }
    #endregion
}
