// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

// Comprehensive tests for protected ToObjectArray methods in generated TestData<>, 
// TestDataReturns<>, and TestDataThrows<> classes. Tests verify proper argument
// marshaling through inheritance chains with various ArgsCode values.

using Portamical.Core.Factories;
using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.TestDataTypes.Models;

[TestClass]
public class ToObjectArrayTests
{
    private const string Def = "test definition";
    private const string Result = "test result";

    #region TestData<> ToObjectArray via ToArgs
    [TestMethod]
    public void TestData_toObjectArray_instance_returnsInstance()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Instance);

        // Assert - Instance mode returns [instance]
        Assert.HasCount(1, result);
        Assert.AreSame(sut, result[0]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_arity1_returnsArg1()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int>(Def, Result, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(1, result);
        Assert.AreEqual(42, result[0]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_arity2_returnsArg1AndArg2()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int, string>(Def, Result, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(2, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("test", result[1]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_arity3_returnsAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int, string, bool>(Def, Result, 42, "test", true);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(3, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("test", result[1]);
        Assert.AreEqual(true, result[2]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_arity4_returnsAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int, string, bool, double>(
            Def, Result, 42, "test", true, 3.14);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(4, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("test", result[1]);
        Assert.AreEqual(true, result[2]);
        Assert.AreEqual(3.14, result[3]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_arity5_returnsAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int, string, bool, double, char>(
            Def, Result, 42, "test", true, 3.14, 'A');

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(5, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("test", result[1]);
        Assert.AreEqual(true, result[2]);
        Assert.AreEqual(3.14, result[3]);
        Assert.AreEqual('A', result[4]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_withAllPropsCode_includesTestCaseName()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<int, string>(Def, Result, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.All);

        // Assert - All includes TestCaseName as first element
        Assert.HasCount(3, result);
        Assert.IsInstanceOfType<string>(result[0]);
        Assert.AreEqual(sut.TestCaseName, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
    }

    [TestMethod]
    public void TestData_toObjectArray_properties_withNullArg_includesNull()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestData<string?, int>(Def, Result, null, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(2, result);
        Assert.IsNull(result[0]);
        Assert.AreEqual(42, result[1]);
    }
    #endregion

    #region TestDataReturns<> ToObjectArray via ToArgs
    [TestMethod]
    public void TestDataReturns_toObjectArray_instance_returnsInstance()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 99, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Instance);

        // Assert
        Assert.HasCount(1, result);
        Assert.AreSame(sut, result[0]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_arity1_returnsExpectedAndArg1()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int>(Def, 99, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(2, result);
        Assert.AreEqual(99, result[0]); // Expected
        Assert.AreEqual(42, result[1]); // Arg1
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_arity2_returnsExpectedAndAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string>(Def, 99, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(3, result);
        Assert.AreEqual(99, result[0]); // Expected
        Assert.AreEqual(42, result[1]); // Arg1
        Assert.AreEqual("test", result[2]); // Arg2
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_arity3_returnsExpectedAndAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string, bool>(
            Def, 99, 42, "test", true);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(4, result);
        Assert.AreEqual(99, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
        Assert.AreEqual(true, result[3]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_arity4_returnsExpectedAndAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string, bool, double>(
            Def, 99, 42, "test", true, 3.14);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(5, result);
        Assert.AreEqual(99, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
        Assert.AreEqual(true, result[3]);
        Assert.AreEqual(3.14, result[4]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_arity5_returnsExpectedAndAllArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string, bool, double, char>(
            Def, 99, 42, "test", true, 3.14, 'X');

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(6, result);
        Assert.AreEqual(99, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
        Assert.AreEqual(true, result[3]);
        Assert.AreEqual(3.14, result[4]);
        Assert.AreEqual('X', result[5]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_withAllPropsCode_includesTestCaseName()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string>(Def, 99, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.All);

        // Assert
        Assert.HasCount(4, result);
        Assert.IsInstanceOfType<string>(result[0]);
        Assert.AreEqual(sut.TestCaseName, result[0]);
        Assert.AreEqual(99, result[1]);
        Assert.AreEqual(42, result[2]);
        Assert.AreEqual("test", result[3]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_trimReturnsExpected_excludesExpectedAndTestCaseName()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int, string>(Def, 99, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);

        // Assert - TrimReturnsExpected removes both TestCaseName and Expected
        Assert.HasCount(2, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("test", result[1]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_properties_withNullArg_includesNull()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, string?, int>(Def, 99, null, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(3, result);
        Assert.AreEqual(99, result[0]);
        Assert.IsNull(result[1]);
        Assert.AreEqual(42, result[2]);
    }
    #endregion

    #region TestDataThrows<> ToObjectArray via ToArgs
    [TestMethod]
    public void TestDataThrows_toObjectArray_instance_returnsInstance()
    {
        // Arrange
        var exception = new ArgumentException("test message");
        var sut = TestDataFactory.CreateTestDataThrows<ArgumentException, int>(Def, exception, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Instance);

        // Assert
        Assert.HasCount(1, result);
        Assert.AreSame(sut, result[0]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_arity1_returnsExpectedAndArg1()
    {
        // Arrange
        var exception = new ArgumentException("test message");
        var sut = TestDataFactory.CreateTestDataThrows<ArgumentException, int>(Def, exception, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(2, result);
        Assert.AreSame(exception, result[0]); // Expected (Exception)
        Assert.AreEqual(42, result[1]); // Arg1
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_arity2_returnsExpectedAndAllArgs()
    {
        // Arrange
        var exception = new InvalidOperationException("test");
        var sut = TestDataFactory.CreateTestDataThrows<InvalidOperationException, int, string>(
            Def, exception, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(3, result);
        Assert.AreSame(exception, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_arity3_returnsExpectedAndAllArgs()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName");
        var sut = TestDataFactory.CreateTestDataThrows<ArgumentNullException, int, string, bool>(
            Def, exception, 42, "test", true);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(4, result);
        Assert.AreSame(exception, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
        Assert.AreEqual(true, result[3]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_arity4_returnsExpectedAndAllArgs()
    {
        // Arrange
        var exception = new ArgumentOutOfRangeException("param");
        var sut = TestDataFactory.CreateTestDataThrows<ArgumentOutOfRangeException, int, string, bool, double>(
            Def, exception, 42, "test", true, 3.14);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(5, result);
        Assert.AreSame(exception, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
        Assert.AreEqual(true, result[3]);
        Assert.AreEqual(3.14, result[4]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_arity5_returnsExpectedAndAllArgs()
    {
        // Arrange
        var exception = new InvalidCastException("cast failed");
        var sut = TestDataFactory.CreateTestDataThrows<InvalidCastException, int, string, bool, double, char>(
            Def, exception, 42, "test", true, 3.14, 'Z');

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(6, result);
        Assert.AreSame(exception, result[0]);
        Assert.AreEqual(42, result[1]);
        Assert.AreEqual("test", result[2]);
        Assert.AreEqual(true, result[3]);
        Assert.AreEqual(3.14, result[4]);
        Assert.AreEqual('Z', result[5]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_withAllPropsCode_includesTestCaseName()
    {
        // Arrange
        var exception = new NotSupportedException();
        var sut = TestDataFactory.CreateTestDataThrows<NotSupportedException, int, string>(
            Def, exception, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.All);

        // Assert
        Assert.HasCount(4, result);
        Assert.IsInstanceOfType<string>(result[0]);
        Assert.AreEqual(sut.TestCaseName, result[0]);
        Assert.AreSame(exception, result[1]);
        Assert.AreEqual(42, result[2]);
        Assert.AreEqual("test", result[3]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_trimThrowsExpected_excludesExpectedAndTestCaseName()
    {
        // Arrange
        var exception = new DivideByZeroException();
        var sut = TestDataFactory.CreateTestDataThrows<DivideByZeroException, int, string>(
            Def, exception, 42, "test");

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);

        // Assert - TrimThrowsExpected removes both TestCaseName and Expected
        Assert.HasCount(2, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("test", result[1]);
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_properties_withNullArg_includesNull()
    {
        // Arrange
        var exception = new ArgumentException("test");
        var sut = TestDataFactory.CreateTestDataThrows<ArgumentException, string?, int>(
            Def, exception, null, 42);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(3, result);
        Assert.AreSame(exception, result[0]);
        Assert.IsNull(result[1]);
        Assert.AreEqual(42, result[2]);
    }
    #endregion

    #region Edge cases and inheritance chain verification
    [TestMethod]
    public void TestData_toObjectArray_arity9_allArgsPresent()
    {
        // Arrange - Test maximum arity (9 args)
        var sut = TestDataFactory.CreateTestData<int, int, int, int, int, int, int, int, int>(
            Def, Result, 1, 2, 3, 4, 5, 6, 7, 8, 9);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert - Verify all 9 arguments marshaled correctly through inheritance chain
        Assert.HasCount(9, result);
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(i + 1, result[i]);
        }
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_arity9_expectedPlusAllArgs()
    {
        // Arrange - Test maximum arity with Expected value
        var sut = TestDataFactory.CreateTestDataReturns<int, int, int, int, int, int, int, int, int, int>(
            Def, 99, 1, 2, 3, 4, 5, 6, 7, 8, 9);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert - Expected + 9 args = 10 elements
        Assert.HasCount(10, result);
        Assert.AreEqual(99, result[0]); // Expected
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(i + 1, result[i + 1]);
        }
    }

    [TestMethod]
    public void TestDataThrows_toObjectArray_arity9_expectedPlusAllArgs()
    {
        // Arrange - Test maximum arity with Exception
        var exception = new Exception("test");
        var sut = TestDataFactory.CreateTestDataThrows<Exception, int, int, int, int, int, int, int, int, int>(
            Def, exception, 1, 2, 3, 4, 5, 6, 7, 8, 9);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert - Exception + 9 args = 10 elements
        Assert.HasCount(10, result);
        Assert.AreSame(exception, result[0]); // Expected (Exception)
        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(i + 1, result[i + 1]);
        }
    }

    [TestMethod]
    public void TestData_toObjectArray_mixedValueAndReferenceTypes()
    {
        // Arrange
        var obj = new object();
        var sut = TestDataFactory.CreateTestData<int, string, object, bool, double>(
            Def, Result, 42, "text", obj, true, 3.14);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert - Verify mixed types handled correctly
        Assert.HasCount(5, result);
        Assert.AreEqual(42, result[0]);
        Assert.AreEqual("text", result[1]);
        Assert.AreSame(obj, result[2]);
        Assert.AreEqual(true, result[3]);
        Assert.AreEqual(3.14, result[4]);
    }

    [TestMethod]
    public void TestDataReturns_toObjectArray_allNullableArgs()
    {
        // Arrange
        var sut = TestDataFactory.CreateTestDataReturns<int, int?, string?, bool?>(
            Def, 99, null, null, null);

        // Act
        var result = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        // Assert
        Assert.HasCount(4, result);
        Assert.AreEqual(99, result[0]);
        Assert.IsNull(result[1]);
        Assert.IsNull(result[2]);
        Assert.IsNull(result[3]);
    }
    #endregion
}
