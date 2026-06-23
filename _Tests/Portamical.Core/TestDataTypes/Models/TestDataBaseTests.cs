// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Tests for TestDataBase public API:
// GetDefinition(), ToArgs(ArgsCode) single-arg overload, and invalid-enum validation.

using Portamical.Core.Factories;
using Portamical.Core.Safety;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Models;
using System.ComponentModel;

namespace Tests.Portamical.Core.TestDataTypes.Models;

[TestClass]
public class TestDataBaseTests
{
    private const string Def = "scenario definition";
    private const string Result = "result";

    private sealed class TestDataBaseChild(string definition) : TestDataBase(definition)
    {

        public override string TestCaseName
        {
            get => throw new NotImplementedException("This branch should not be reached in tests.");
            init => throw new NotImplementedException("This branch should not be reached in tests.");
        }

        public override string GetResult()
        {
            throw new NotImplementedException("This branch should not be reached in tests.");
        }

        public static object?[] Extend<T>(
            ArgsCode argsCode,
            T? newArg)
        => Extend(
            baseToObjectArray: (argsCode) => [],
            argsCode,
            newArg);

        protected override object?[] ToObjectArray(ArgsCode argsCode)
        => [];
    }

    #region GetDefinition
    [TestMethod]
    public void GetDefinition_returnsDefinitionString()
    {
        var sut = TestDataFactory.CreateTestData(Def, Result, 1);
        Assert.AreEqual(Def, sut.GetDefinition());
    }

    [TestMethod]
    public void GetDefinition_forTestDataReturns_returnsDefinitionString()
    {
        var sut = TestDataFactory.CreateTestDataReturns(Def, 5, 1);
        Assert.AreEqual(Def, sut.GetDefinition());
    }

    [TestMethod]
    public void GetDefinition_forTestDataThrows_returnsDefinitionString()
    {
        var sut = TestDataFactory.CreateTestDataThrows(Def, new InvalidOperationException(), 1);
        Assert.AreEqual(Def, sut.GetDefinition());
    }
    #endregion

    #region ToArgs single-arg overload (uses PropsCode.TrimTestCaseName by default)
    [TestMethod]
    public void ToArgs_instance_singleArgOverload_equals_twoArgVersion_withAnyPropsCode()
    {
        var sut = TestDataFactory.CreateTestData(Def, Result, 42);
        var oneArg = sut.ToArgs(ArgsCode.Instance);
        var twoArg = sut.ToArgs(ArgsCode.Instance, PropsCode.All);
        // Instance mode ignores PropsCode — both should return [sut]
        Assert.HasCount(1, oneArg);
        Assert.AreSame(sut, oneArg[0]);
        Assert.HasCount(1, twoArg);
        Assert.AreSame(sut, twoArg[0]);
    }

    [TestMethod]
    public void ToArgs_properties_singleArgOverload_forReturns_usesTrimTestCaseName()
    {
        var sut = TestDataFactory.CreateTestDataReturns(Def, 5, 42);
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
        var sut = TestDataFactory.CreateTestData(Def, Result, 1);
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => sut.ToArgs(ArgsCode.Properties, (PropsCode)99));
    }

    [TestMethod]
    public void ToArgs_withUndefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        var sut = TestDataFactory.CreateTestData(Def, Result, 1);
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => sut.ToArgs((ArgsCode)99, PropsCode.All));
    }

    [TestMethod]
    public void Extend_withUndefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => _ = TestDataBaseChild.Extend((ArgsCode)99, new object()));
    }

    [TestMethod]
    public void ToArgs_withZeroElements_throwsInvalidOperationException()
    {
        var sut = new TestDataBaseChild(Def);
        Assert.ThrowsExactly<InvalidOperationException>(
            () => _ = sut.ToArgs(ArgsCode.Instance));
        Assert.ThrowsExactly<InvalidOperationException>(
            () => _ = sut.ToArgs(ArgsCode.Properties, default));
    }
    #endregion

    #region ToArgs successful path - non-empty array returned
    [TestMethod]
    public void ToArgs_properties_withValidPropsCode_returnsNonEmptyArray()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert - verifies false branch (args.Length != 0) returns args
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual(42, result[0]);
    }

    [TestMethod]
    public void ToArgs_properties_withAllPropsCode_returnsNonEmptyArrayIncludingTestCaseName()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.All);

        // Assert - verifies false branch (args.Length != 0) returns args
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsInstanceOfType<string>(result[0]); // TestCaseName
        Assert.AreEqual(42, result[1]);
    }

    [TestMethod]
    public void ToArgs_instance_returnsNonEmptyArray()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Instance, PropsCode.TrimTestCaseName);

        // Assert - verifies false branch (args.Length != 0) returns args
        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreSame(sut, result[0]);
    }
    #endregion
}
