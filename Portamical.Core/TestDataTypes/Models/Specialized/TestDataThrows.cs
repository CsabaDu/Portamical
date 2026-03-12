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
///   <item><strong>Exception-specific operations:</strong> Enables access to <see cref="Exception"/> members like <see cref="Exception.Message"/> and <see cref="Type.Name"/> for concise test case names</item>
/// </list>
/// </para>
/// <para>
/// <strong>Counterpart:</strong> This class is the exception-testing counterpart to <see cref="TestDataReturns{TStruct}"/>,
/// which handles return value testing. Together, they provide comprehensive test data capabilities for both
/// success paths (returns) and failure paths (throws).
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Implements <see cref="IThrows{TException}"/> marker interface for type discrimination</item>
///   <item>Returns "throws" as the result prefix via <see cref="GetResultPrefix()"/></item>
///   <item>Formats expected exception as "throws {exceptionType}" via <see cref="GetResult()"/> using <see cref="Type.Name"/></item>
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
///     TestCaseName = "Validate(null) =&gt; throws ArgumentException",
///     Expected = new ArgumentException("Value cannot be null", "input"),
///     Arg1 = null
/// };
/// // Test case name: "Validate(null) => throws ArgumentException"
/// 
/// // Testing InvalidOperationException
/// var invalidOpTest = new TestDataThrows&lt;InvalidOperationException&gt;
/// {
///     TestCaseName = "Operation when closed =&gt; throws InvalidOperationException",
///     Expected = new InvalidOperationException("Cannot perform operation on closed object")
/// };
/// // Test case name: "Operation when closed => throws InvalidOperationException"
/// 
/// // Testing custom exception
/// public class ValidationException : Exception
/// {
///     public ValidationException(string message) : base(message) { }
/// }
/// 
/// var customTest = new TestDataThrows&lt;ValidationException, int&gt;
/// {
///     TestCaseName = "Validate(-1) =&gt; throws ValidationException",
///     Expected = new ValidationException("Value must be positive"),
///     Arg1 = -1
/// };
/// // Test case name: "Validate(-1) => throws ValidationException"
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

    private const string ThrowsString = "throws";

    /// <inheritdoc/>
    public override sealed string GetResultPrefix()
    => GetValidResultPrefix(ThrowsString);

    /// <summary>
    /// Gets the formatted result string for the test case name.
    /// </summary>
    /// <returns>
    /// A formatted string in the form <c>"throws {exceptionType}"</c>, where <c>{exceptionType}</c>
    /// is the name of the exception type (without namespace) from <see cref="TestDataExpected{TResult}.Expected"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataBase.GetResult()"/> to provide the expected
    /// outcome portion of the test case name for exception tests. It calls 
    /// <see cref="TestDataExpected{TResult}.GetExpectedResult(string?)"/> with the exception type name
    /// (via <see cref="Type.Name"/>) rather than the full exception details.
    /// </para>
    /// <para>
    /// <strong>Type Name vs ToString():</strong> This method uses <see cref="Type.Name"/> instead of
    /// <see cref="Exception.ToString()"/> to produce concise test case names. 
    /// <see cref="Exception.ToString()"/> includes the full type name, message, and stack trace,
    /// which would be too verbose for test case naming.
    /// </para>
    /// <para>
    /// <strong>Example:</strong> For <c>Expected = new ArgumentException("Invalid input", "param")</c>,
    /// this returns <c>"throws ArgumentException"</c>, resulting in a test case name like
    /// <c>"Validate(null) =&gt; throws ArgumentException"</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // ArgumentException
    /// var argTest = new TestDataThrows&lt;ArgumentException&gt;(
    ///     "Validate(null)", 
    ///     new ArgumentException("Value cannot be null", "input"));
    /// string result = argTest.GetResult();
    /// // Returns: "throws ArgumentException" ✅ Concise
    /// // NOT: "throws System.ArgumentException: Value cannot be null (Parameter 'input')" ❌ Too verbose
    /// 
    /// // InvalidOperationException
    /// var invalidOpTest = new TestDataThrows&lt;InvalidOperationException&gt;(
    ///     "Operation when closed", 
    ///     new InvalidOperationException("Cannot perform operation on closed object"));
    /// string result2 = invalidOpTest.GetResult();
    /// // Returns: "throws InvalidOperationException" ✅ Concise
    /// 
    /// // Custom exception
    /// public class CustomException : Exception
    /// {
    ///     public CustomException(string message) : base(message) { }
    /// }
    /// 
    /// var customTest = new TestDataThrows&lt;CustomException&gt;(
    ///     "Custom scenario", 
    ///     new CustomException("Custom error"));
    /// string result3 = customTest.GetResult();
    /// // Returns: "throws CustomException" ✅ Works with any Exception-derived type
    /// </code>
    /// </example>
    public override sealed string GetResult()
    => GetExpectedResult(Expected.GetType().Name);

    /// <inheritdoc/>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimThrowsExpected);
}
