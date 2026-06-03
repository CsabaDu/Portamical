// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

/// <summary>
/// Abstract base class for test data that verifies exception throwing behavior.
/// </summary>
/// <typeparam name="TException">
/// The type of exception expected to be thrown. Must derive from <see cref="Exception"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="TestDataExpected{TResult}"/> and implements <see cref="IThrows{TException}"/>
/// to provide a foundation for test data types that verify error handling and exceptional execution paths.
/// </para>
/// <para>
/// <strong>Constraint Rationale:</strong> The <c>Exception</c> constraint ensures type safety
/// and provides key benefits:
/// <list type="number">
///   <item><strong>Type safety:</strong> Only throwable types are allowed, preventing compilation with non-exception types</item>
///   <item><strong>Exception hierarchy support:</strong> Supports the entire .NET exception hierarchy, including custom exceptions</item>
///   <item><strong>Exception-specific formatting:</strong> Enables access to <see cref="Exception"/> members for concise test case names via the base class formatting</item>
/// </list>
/// </para>
/// <para>
/// <strong>Result Formatting:</strong> This class provides the result prefix "throws" via <see cref="GetResultPrefix()"/>.
/// The base class <see cref="TestDataExpected{TResult}.GetResult"/> combines this prefix with the formatted
/// exception to create test case names like: <c>"Validate(null) =&gt; throws ArgumentException: Value cannot be null"</c>.
/// The base class Format method specifically handles exceptions by showing type name and message.
/// </para>
/// <para>
/// <strong>Counterpart:</strong> This class is the exception-testing counterpart to <see cref="TestDataReturns{TResult}"/>,
/// which handles return value testing. Together, they provide comprehensive test data capabilities for both
/// success paths (returns) and failure paths (throws).
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Implements <see cref="IThrows{TException}"/> marker interface for type discrimination</item>
///   <item>Returns "throws" as the result prefix via <see cref="GetResultPrefix()"/></item>
///   <item>Inherits intelligent exception formatting from base class: "ExceptionType: Message"</item>
///   <item>Supports trimming of expected exception via <see cref="PropsCode.TrimThrowsExpected"/></item>
/// </list>
/// </para>
/// <para>
/// <strong>Derived Types:</strong> Further derived classes add argument properties (e.g.,
/// <c>TestDataThrows&lt;TException, TArg1&gt;</c>, <c>TestDataThrows&lt;TException, TArg1, TArg2&gt;</c>).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Testing ArgumentException
/// var argTest = new TestDataThrows&lt;ArgumentException, string&gt;
/// {
///     TestCaseName = "Validate(null) =&gt; throws ArgumentException: Value cannot be null",
///     Expected = new ArgumentException("Value cannot be null", "input"),
///     Arg1 = null
/// };
/// // Formatted: "Validate(null) => throws ArgumentException: Value cannot be null" ✅
/// 
/// // Testing InvalidOperationException
/// var invalidOpTest = new TestDataThrows&lt;InvalidOperationException&gt;
/// {
///     TestCaseName = "Operation when closed =&gt; throws InvalidOperationException: Cannot perform operation on closed object",
///     Expected = new InvalidOperationException("Cannot perform operation on closed object")
/// };
/// // Shows exception type and message ✅
/// 
/// // Testing custom exception
/// public class ValidationException : Exception
/// {
///     public ValidationException(string message) : base(message) { }
/// }
/// 
/// var customTest = new TestDataThrows&lt;ValidationException, int&gt;
/// {
///     TestCaseName = "Validate(-1) =&gt; throws ValidationException: Value must be positive",
///     Expected = new ValidationException("Value must be positive"),
///     Arg1 = -1
/// };
/// // Custom exceptions formatted the same way ✅
/// </code>
/// </example>
public abstract class TestDataThrows<TException>
: TestDataExpected<TException>,
IThrows<TException>
where TException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataThrows{TException}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The descriptive definition of the test case scenario (left side of "=&gt;").
    /// </param>
    /// <param name="expected">
    /// The expected exception instance. Should include appropriate <see cref="Exception.Message"/> and
    /// other exception properties for test verification.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Accessibility:</strong> This constructor is <c>private protected</c> to prevent
    /// derivation outside the Portamical assembly. Users should use the provided concrete
    /// implementations (e.g., <c>TestDataThrows&lt;TException, TArg1&gt;</c>) rather than
    /// deriving custom classes.
    /// </para>
    /// <para>
    /// <strong>Exception Instance:</strong> The <paramref name="expected"/> parameter should be
    /// a fully-constructed exception instance with appropriate message, parameter names (for
    /// <see cref="ArgumentException"/>), and other properties. This allows test assertions to
    /// verify not just the exception type, but also its details.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Constructor called by derived class:
    /// var test = new TestDataThrows&lt;ArgumentException, string&gt;(
    ///     definition: "Validate(null)",
    ///     expected: new ArgumentException("Value cannot be null", "input")
    /// );
    /// </code>
    /// </example>
    private protected TestDataThrows(
        string definition,
        TException expected)
    : base(definition, expected)
    {
    }

    /// <summary>
    /// Gets the result prefix for exception throwing test cases.
    /// </summary>
    /// <returns>The string "throws".</returns>
    /// <remarks>
    /// <para>
    /// This method provides the distinguishing prefix for exception test cases. The base class
    /// <see cref="TestDataExpected{TResult}.GetResult"/> combines this prefix with the formatted
    /// exception to create complete test case names.
    /// </para>
    /// <para>
    /// <strong>Example:</strong> For a test with <c>Expected = new ArgumentException("Invalid")</c>,
    /// the base class generates: <c>"throws ArgumentException: Invalid"</c> by combining this prefix
    /// with the formatted exception.
    /// </para>
    /// </remarks>
    public override sealed string GetResultPrefix()
    => "throws";

    /// <summary>
    /// Converts the test data to a parameter array with optional trimming of the expected exception.
    /// </summary>
    /// <param name="argsCode">Determines whether to include the instance itself or its properties.</param>
    /// <param name="propsCode">Specifies which properties to include when using <see cref="ArgsCode.Properties"/>.</param>
    /// <returns>
    /// A parameter array, with the first element (expected exception) removed when
    /// <paramref name="propsCode"/> is <see cref="PropsCode.TrimThrowsExpected"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataExpected{TResult}.ToArgs"/> to provide specialized
    /// trimming logic for exception tests. When <see cref="PropsCode.TrimThrowsExpected"/> is specified,
    /// the expected exception is excluded from the argument array, which is useful when the test
    /// framework expects only the method arguments (not the expected exception).
    /// </para>
    /// <para>
    /// <strong>Trimming Logic:</strong>
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Keeps expected exception (no trim)</item>
    ///   <item><see cref="PropsCode.TrimTestCaseName"/> - Keeps expected exception (no trim)</item>
    ///   <item><see cref="PropsCode.TrimReturnsExpected"/> - Keeps expected exception (no trim, wrong type)</item>
    ///   <item><see cref="PropsCode.TrimThrowsExpected"/> - Removes expected exception (trim) ✅</item>
    /// </list>
    /// </para>
    /// </remarks>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimThrowsExpected);
}
