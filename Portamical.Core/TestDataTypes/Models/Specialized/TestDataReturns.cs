// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

/// <summary>
/// Abstract base class for test data that verifies method return values.
/// </summary>
/// <typeparam name="TResult">
/// The type of the expected return value. Must be not null.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="TestDataExpected{TResult}"/> and implements <see cref="IReturns{TResult}"/>
/// to provide a foundation for test data types that verify successful execution paths with return values.
/// </para>
/// <para>
/// <strong>Constraint Rationale:</strong> The <c>notnull</c> constraint ensures that:
/// <list type="number">
///   <item><strong>Non-null guarantee:</strong> The expected return value cannot be null, ensuring test expectations are always concrete values</item>
///   <item><strong>Meaningful formatting:</strong> Non-null types can be formatted reliably by the base class for readable test case names</item>
///   <item><strong>Type flexibility:</strong> Supports both value types and reference types (including strings, objects, custom classes) that are non-null</item>
/// </list>
/// </para>
/// <para>
/// <strong>Result Formatting:</strong> This class provides the result prefix "returns" via <see cref="GetResultPrefix()"/>.
/// The base class <see cref="TestDataExpected{TResult}.GetResult"/> combines this prefix with the formatted
/// <see cref="TestDataExpected{TResult}.Expected"/> value to create test case names like:
/// <c>"Add(2,3) =&gt; returns 5"</c> or <c>"GetName() =&gt; returns \"John\""</c>.
/// See <see cref="TestDataExpected{TResult}"/> for details on type-specific formatting (char, DateTime, collections, etc.).
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Implements <see cref="IReturns{TResult}"/> marker interface for type discrimination</item>
///   <item>Returns "returns" as the result prefix via <see cref="GetResultPrefix()"/></item>
///   <item>Inherits intelligent formatting from base class for char, DateTime, Guid, collections, exceptions, etc.</item>
///   <item>Supports trimming of expected value via <see cref="PropsCode.TrimReturnsExpected"/></item>
/// </list>
/// </para>
/// <para>
/// <strong>Derived Types:</strong> Further derived classes add argument properties (e.g.,
/// <c>TestDataReturns&lt;TResult, TArg1&gt;</c>, <c>TestDataReturns&lt;TResult, TArg1, TArg2&gt;</c>).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Testing integer return values
/// var intTest = new TestDataReturns&lt;int, int, int&gt;
/// {
///     TestCaseName = "Add(2,3) =&gt; returns 5",
///     Expected = 5,  // Guaranteed non-null
///     Arg1 = 2,
///     Arg2 = 3
/// };
/// // Test case name: "Add(2,3) => returns 5" ✅
/// 
/// // Testing string return values (with quotes in formatted output)
/// var strTest = new TestDataReturns&lt;string, string&gt;
/// {
///     TestCaseName = "GetName(\"John\") =&gt; returns \"John\"",
///     Expected = "John",  // Guaranteed non-null
///     Arg1 = "John"
/// };
/// // Test case name: "GetName(\"John\") => returns \"John\"" ✅
/// 
/// // Testing DateTime return values (ISO 8601 formatExpected)
/// var dateTest = new TestDataReturns&lt;DateTime&gt;
/// {
///     TestCaseName = "Now() =&gt; returns 2026-01-15T10:30:00.0000000Z",
///     Expected = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
/// };
/// // Formatted with ISO 8601 for precision ✅
/// 
/// // Testing collection return values
/// var listTest = new TestDataReturns&lt;List&lt;int&gt;&gt;
/// {
///     TestCaseName = "GetNumbers() =&gt; returns [3]: [1, 2, 3]",
///     Expected = new List&lt;int&gt; { 1, 2, 3 }  // Formatted by base class
/// };
/// // Shows count and items ✅
/// </code>
/// </example>
public abstract class TestDataReturns<TResult>
: TestDataExpected<TResult>,
IReturns<TResult>
where TResult : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataReturns{TResult}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The descriptive definition of the test case scenario (left side of "=&gt;").
    /// </param>
    /// <param name="expected">
    /// The expected return value. Guaranteed to be non-null due to <c>notnull</c> constraint.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Accessibility:</strong> This constructor is <c>private protected</c> to prevent
    /// derivation outside the Portamical assembly. Users should use the provided concrete
    /// implementations (e.g., <c>TestDataReturns&lt;TResult, TArg1&gt;</c>) rather than
    /// deriving custom classes.
    /// </para>
    /// <para>
    /// <strong>Non-null Guarantee:</strong> The <c>notnull</c> constraint ensures <paramref name="expected"/>
    /// can never be null, eliminating null-reference concerns in test assertions.
    /// </para>
    /// </remarks>
    private protected TestDataReturns(
        string definition,
        TResult expected)
    : base(definition, expected)
    {
    }

    /// <summary>
    /// Gets the result prefix for return value test cases.
    /// </summary>
    /// <returns>The string "returns".</returns>
    /// <remarks>
    /// <para>
    /// This method provides the distinguishing prefix for return value test cases. The base class
    /// <see cref="TestDataExpected{TResult}.GetResult"/> combines this prefix with the formatted
    /// <see cref="TestDataExpected{TResult}.Expected"/> value to create complete test case names.
    /// </para>
    /// <para>
    /// <strong>Example:</strong> For a test with <c>Expected = 5</c>, the base class generates:
    /// <c>"returns 5"</c> by combining this prefix with the formatted value.
    /// </para>
    /// </remarks>
    public override sealed string GetResultPrefix()
    => "returns";

    /// <summary>
    /// Converts the test data to a parameter array with optional trimming of the expected return value.
    /// </summary>
    /// <param name="argsCode">Determines whether to include the instance itself or its properties.</param>
    /// <param name="propsCode">Specifies which properties to include when using <see cref="ArgsCode.Properties"/>.</param>
    /// <returns>
    /// A parameter array, with the first element (expected return value) removed when
    /// <paramref name="propsCode"/> is <see cref="PropsCode.TrimReturnsExpected"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataExpected{TResult}.ToArgs"/> to provide specialized
    /// trimming logic for return value tests. When <see cref="PropsCode.TrimReturnsExpected"/> is specified,
    /// the expected return value is excluded from the argument array, which is useful when the test
    /// framework expects only the method arguments (not the expected result).
    /// </para>
    /// <para>
    /// <strong>Trimming Logic:</strong>
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Keeps expected value (no trim)</item>
    ///   <item><see cref="PropsCode.TrimTestCaseName"/> - Keeps expected value (no trim)</item>
    ///   <item><see cref="PropsCode.TrimReturnsExpected"/> - Removes expected value (trim) ✅</item>
    ///   <item><see cref="PropsCode.TrimThrowsExpected"/> - Keeps expected value (no trim, wrong type)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimReturnsExpected);
}
