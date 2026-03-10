// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Strategy;

/// <summary>
/// Specifies the serialization strategy for converting test data to method arguments.
/// </summary>
/// <remarks>
/// <para>
/// This enum implements the Strategy pattern for test argument generation. It determines
/// whether test methods receive the complete <see cref="ITestData"/> instance or individual
/// property values as parameters.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Strategy Enum - enables compile-time strategy selection
/// for test data serialization without complex class hierarchies.
/// </para>
/// <para>
/// <strong>Usage Context:</strong> Used by <see cref="ITestData.ToArgs(ArgsCode)"/> and
/// <see cref="IDataStrategy"/> implementations to determine argument array structure.
/// </para>
/// <para>
/// <strong>Framework Integration:</strong>
/// <list type="bullet">
///   <item><strong>xUnit:</strong> Both strategies supported via <c>MemberData</c> and <c>ClassData</c></item>
///   <item><strong>NUnit:</strong> Both strategies supported via <c>TestCaseSource</c></item>
///   <item><strong>MSTest:</strong> Both strategies supported via <c>DynamicData</c></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Strategy selection in test data class
/// public class TestData&lt;TArg1, TArg2&gt; : ITestData
/// {
///     public object?[] ToArgs(ArgsCode argsCode)
///     {
///         return argsCode switch
///         {
///             ArgsCode.Instance =&gt; [this],           // Return entire object
///             ArgsCode.Properties =&gt; [Arg1, Arg2],   // Return flattened properties
///             _ =&gt; throw new InvalidEnumArgumentException(...)
///         };
///     }
/// }
/// 
/// // Test method with Instance strategy
/// [Theory, MemberData(nameof(TestCases))]
/// public void Test(TestData&lt;int, int&gt; testData)
/// {
///     int result = Add(testData.Arg1, testData.Arg2);
///     Assert.Equal(testData.Expected, result);
/// }
/// 
/// // Test method with Properties strategy
/// [Theory, MemberData(nameof(TestCases))]
/// public void Test(int arg1, int arg2)
/// {
///     int result = Add(arg1, arg2);
///     Assert.True(result &gt; 0);
/// }
/// </code>
/// </example>
public enum ArgsCode
{
    /// <summary>
    /// Serializes the complete <see cref="TestDataTypes.ITestData"/> instance as a single argument.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Resulting Array:</strong> <c>[testDataInstance]</c> - single element containing the entire test data object.
    /// </para>
    /// <para>
    /// <strong>PropsCode Behavior:</strong> When <see cref="Instance"/> is used,
    /// <see cref="PropsCode"/> values are ignored because the entire object is passed.
    /// </para>
    /// <para>
    /// <strong>Test Method Signature:</strong>
    /// <code>
    /// public void TestMethod(TestData&lt;TArg&gt; testData)
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Benefits:</strong>
    /// <list type="bullet">
    ///   <item>Access to all test data properties and methods</item>
    ///   <item>Can use <c>GetDefinition()</c>, <c>GetResult()</c> for assertions</item>
    ///   <item>Single parameter (clean signature)</item>
    ///   <item>Access to <c>TestCaseName</c> for dynamic messages</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Use When:</strong>
    /// <list type="bullet">
    ///   <item>Need access to test metadata (test case name, definition, result)</item>
    ///   <item>Want to call test data methods in test logic</item>
    ///   <item>Prefer object-oriented test method signatures</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Test data class
    /// var testData = new TestData&lt;int, int&gt;
    /// {
    ///     TestCaseName = "Add(2,3) =&gt; returns 5",
    ///     Arg1 = 2,
    ///     Arg2 = 3,
    ///     Expected = 5
    /// };
    /// 
    /// // Serialization
    /// var args = testData.ToArgs(ArgsCode.Instance);
    /// // Result: [testData] (single element)
    /// 
    /// // Test method
    /// [Theory, MemberData(nameof(TestCases))]
    /// public void Add_ValidInputs_ReturnsSum(TestData&lt;int, int&gt; testData)
    /// {
    ///     int result = Calculator.Add(testData.Arg1, testData.Arg2);
    ///     
    ///     Assert.Equal(testData.Expected, result);
    ///     
    ///     // Can access metadata
    ///     Console.WriteLine($"Test: {testData.TestCaseName}");
    ///     Console.WriteLine($"Definition: {testData.GetDefinition()}");
    /// }
    /// </code>
    /// </example>
    Instance,

    /// <summary>
    /// Serializes individual test data properties as separate arguments (flattened representation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Resulting Array:</strong> <c>[arg1, arg2, ..., expected]</c> - multiple elements, one per property.
    /// </para>
    /// <para>
    /// <strong>PropsCode Behavior:</strong> When <see cref="Properties"/> is used,
    /// the <see cref="PropsCode"/> value determines which properties are included:
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Includes TestCaseName, args, and expected value</item>
    ///   <item><see cref="PropsCode.TrimTestCaseName"/> - Excludes TestCaseName (most common)</item>
    ///   <item><see cref="PropsCode.TrimReturnsExpected"/> - Excludes Expected return value (for method params only)</item>
    ///   <item><see cref="PropsCode.TrimThrowsExpected"/> - Excludes Expected exception (for method params only)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Test Method Signature:</strong>
    /// <code>
    /// public void TestMethod(int arg1, int arg2, int expected)
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Benefits:</strong>
    /// <list type="bullet">
    ///   <item>Typed parameters (no casting required)</item>
    ///   <item>Clear test method signature</item>
    ///   <item>IntelliSense support for parameters</item>
    ///   <item>Works with framework display name features</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Use When:</strong>
    /// <list type="bullet">
    ///   <item>Want typed, individual parameters in test method</item>
    ///   <item>Don't need access to test metadata</item>
    ///   <item>Prefer functional test method signatures</item>
    ///   <item>Using framework-specific display name features (e.g., MSTest's DynamicDataDisplayName)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Test data class
    /// var testData = new TestData&lt;int, int&gt;
    /// {
    ///     TestCaseName = "Add(2,3) =&gt; returns 5",
    ///     Arg1 = 2,
    ///     Arg2 = 3,
    ///     Expected = 5
    /// };
    /// 
    /// // Serialization with TrimTestCaseName (most common)
    /// var args = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
    /// // Result: [2, 3, 5] (three elements)
    /// 
    /// // Test method
    /// [Theory, MemberData(nameof(TestCases))]
    /// public void Add_ValidInputs_ReturnsSum(int arg1, int arg2, int expected)
    /// {
    ///     // Typed parameters - no casting needed
    ///     int result = Calculator.Add(arg1, arg2);
    ///     
    ///     Assert.Equal(expected, result);
    ///     
    ///     // Note: Cannot access test case name or metadata
    /// }
    /// 
    /// // MSTest with TestCaseName included
    /// var argsWithName = testData.ToArgs(ArgsCode.Properties, PropsCode.All);
    /// // Result: ["Add(2,3) => returns 5", 2, 3, 5]
    /// 
    /// [DataTestMethod, DynamicData(nameof(TestCases), DynamicDataDisplayName = nameof(GetDisplayName))]
    /// public void Add_ValidInputs_ReturnsSum(string testCaseName, int arg1, int arg2, int expected)
    /// {
    ///     // First parameter is test case name for display
    ///     int result = Calculator.Add(arg1, arg2);
    ///     Assert.Equal(expected, result);
    /// }
    /// </code>
    /// </example>
    Properties,
}