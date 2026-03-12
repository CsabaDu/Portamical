// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity;
using Portamical.Core.Strategy;

namespace Portamical.Core.TestDataTypes;

/// <summary>
/// Core interface representing test data with basic test case functionality.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="INamedCase"/> to provide a complete test data abstraction.
/// Implementations typically represent data-driven test cases with scenario definitions and
/// expected outcomes.
/// </para>
/// <para>
/// <strong>Provides fundamental operations for:</strong>
/// <list type="bullet">
///   <item>Test case naming and identification (via <see cref="INamedCase"/>)</item>
///   <item>Test scenario definition via <see cref="GetDefinition()"/></item>
///   <item>Expected result specification via <see cref="GetResult()"/></item>
///   <item>Argument generation for test execution via <see cref="ToArgs(ArgsCode)"/> and <see cref="ToArgs(ArgsCode, PropsCode)"/></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> This interface implements the Strategy pattern for argument generation,
/// allowing flexible conversion of test data to method parameters based on <see cref="ArgsCode"/> and <see cref="PropsCode"/>.
/// </para>
/// <para>
/// <strong>Test Case Name Format:</strong> The <see cref="INamedCase.TestCaseName"/> typically follows the format:
/// <c>"{scenario description} =&gt; {expected outcome}"</c>
/// <br/>
/// <see cref="GetDefinition()"/> returns the scenario part, and <see cref="GetResult()"/> returns the outcome part.
/// </para>
/// <para>
/// <strong>Typical Usage:</strong> Implement this interface for custom test data types, or use
/// built-in implementations like <c>TestData&lt;TArg&gt;</c>, <c>TestDataReturns&lt;TExpected, TArg&gt;</c>, etc.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Using ITestData in a test
/// public class CalculatorTests
/// {
///     [Theory, MemberData(nameof(TestCases))]
///     public void Add_ValidInputs_ReturnsSum(TestData&lt;int, int&gt; testData)
///     {
///         // Access: testData.Arg1, testData.Arg2
///         int result = Calculator.Add(testData.Arg1, testData.Arg2);
///         Assert.True(result > 0);
///     }
///     
///     public static IEnumerable&lt;object[]&gt; TestCases
///     {
///         get
///         {
///             yield return new object[] { new TestData&lt;int, int&gt;
///             {
///                 TestCaseName = "Adding positives =&gt; returns sum",
///                 Arg1 = 2,
///                 Arg2 = 3
///             }};
///         }
///     }
/// }
/// </code>
/// </example>
public interface ITestData : INamedCase
{
    /// <summary>
    /// Gets the description of the test scenario being verified.
    /// </summary>
    /// <returns>
    /// A string describing the test scenario, typically the left side of the test case name
    /// (before "=&gt;").
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method typically returns the scenario portion of the test case name format:
    /// <c>"scenario description =&gt; expected outcome"</c>
    /// </para>
    /// <para>
    /// <strong>Example:</strong> For a test case named "Adding two positive numbers =&gt; returns their sum",
    /// this method would return "Adding two positive numbers".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestData&lt;int, int&gt;
    /// {
    ///     TestCaseName = "Valid input =&gt; succeeds",
    ///     Arg1 = 5,
    ///     Arg2 = 10
    /// };
    /// 
    /// string definition = testData.GetDefinition();
    /// // Returns: "Valid input"
    /// </code>
    /// </example>
    string GetDefinition();

    /// <summary>
    /// Gets the expected result or outcome of the test scenario.
    /// </summary>
    /// <returns>
    /// A string describing what the test expects to happen or achieve.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method typically returns the outcome portion of the test case name format:
    /// <c>"scenario description =&gt; expected outcome"</c>
    /// </para>
    /// <para>
    /// <strong>Example:</strong> For a test case named "Adding two positive numbers =&gt; returns their sum",
    /// this method would return "returns their sum".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestData&lt;int, int&gt;
    /// {
    ///     TestCaseName = "Valid input =&gt; succeeds",
    ///     Arg1 = 5,
    ///     Arg2 = 10
    /// };
    /// 
    /// string result = testData.GetResult();
    /// // Returns: "succeeds"
    /// </code>
    /// </example>
    string GetResult();

    /// <summary>
    /// Converts the test data into an array of argument values based on the specified serialization strategy.
    /// </summary>
    /// <param name="argsCode">
    /// The argument serialization strategy:
    /// <list type="bullet">
    ///   <item><see cref="ArgsCode.Instance"/> - Returns the test data object itself</item>
    ///   <item><see cref="ArgsCode.Properties"/> - Returns flattened property values</item>
    /// </list>
    /// </param>
    /// <returns>
    /// An array of objects representing the test arguments. The array may contain null values
    /// depending on the test data properties.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This overload uses default <see cref="PropsCode"/> behavior (typically <see cref="PropsCode.TrimTestCaseName"/>).
    /// For precise control over included properties, use <see cref="ToArgs(ArgsCode, PropsCode)"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestData&lt;int, int&gt;
    /// {
    ///     TestCaseName = "test =&gt; succeeds",
    ///     Arg1 = 5,
    ///     Arg2 = 10
    /// };
    /// 
    /// // Instance mode - returns test data object
    /// var args1 = testData.ToArgs(ArgsCode.Instance);
    /// // Result: [testData]
    /// 
    /// // Properties mode - returns flattened values
    /// var args2 = testData.ToArgs(ArgsCode.Properties);
    /// // Result: [5, 10]
    /// </code>
    /// </example>
    object?[] ToArgs(ArgsCode argsCode);

    /// <summary>
    /// Converts the test data into an array of argument values with precise control over
    /// serialization strategy and included properties.
    /// </summary>
    /// <param name="argsCode">
    /// The argument serialization strategy:
    /// <list type="bullet">
    ///   <item><see cref="ArgsCode.Instance"/> - Returns the test data object itself</item>
    ///   <item><see cref="ArgsCode.Properties"/> - Returns flattened property values</item>
    /// </list>
    /// </param>
    /// <param name="propsCode">
    /// Specifies which properties to include in the result:
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Includes all properties (TestCaseName, args, expected)</item>
    ///   <item><see cref="PropsCode.TrimTestCaseName"/> - Excludes TestCaseName property</item>
    ///   <item><see cref="PropsCode.TrimReturnsExpected"/> - Excludes Expected property (for TestDataReturns)</item>
    ///   <item><see cref="PropsCode.TrimThrowsExpected"/> - Excludes Expected exception (for TestDataThrows)</item>
    /// </list>
    /// </param>
    /// <returns>
    /// An array of objects representing the test arguments, filtered according to <paramref name="propsCode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>When argsCode is Instance:</strong> <paramref name="propsCode"/> typically has no effect, as the entire
    /// test data object is returned.
    /// </para>
    /// <para>
    /// <strong>When argsCode is Properties:</strong> <paramref name="propsCode"/> determines which properties are
    /// flattened into the result array.
    /// </para>
    /// <para>
    /// <strong>Framework-Specific Usage:</strong>
    /// <list type="bullet">
    ///   <item>MSTest typically uses <see cref="PropsCode.All"/> to include TestCaseName for display names</item>
    ///   <item>xUnit/NUnit typically use <see cref="PropsCode.TrimTestCaseName"/> as display names are handled separately</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestDataReturns&lt;int, int, int&gt;
    /// {
    ///     TestCaseName = "Add test =&gt; returns sum",
    ///     Expected = 15,
    ///     Arg1 = 5,
    ///     Arg2 = 10
    /// };
    /// 
    /// // Include all properties
    /// var args1 = testData.ToArgs(ArgsCode.Properties, PropsCode.All);
    /// // Result: ["Add test => returns sum", 5, 10, 15]
    /// 
    /// // Exclude TestCaseName
    /// var args2 = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
    /// // Result: [5, 10, 15]
    /// 
    /// // Exclude Expected (for method parameters only)
    /// var args3 = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
    /// // Result: [5, 10]
    /// </code>
    /// </example>
    object?[] ToArgs(ArgsCode argsCode, PropsCode propsCode);
}