// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using TUnit.Assertions.Exceptions;

namespace Portamical.TUnit.Assertions;

/// <summary>
/// Provides TUnit-specific assertion helper methods extending the framework-agnostic base class.
/// </summary>
/// <remarks>
/// <para>
/// This class adapts the framework-agnostic assertion logic from <see cref="Portamical.Assertions.PortamicalAssert"/>
/// to work seamlessly with TUnit's assertion framework. It provides simplified APIs that automatically wire up
/// TUnit-specific assertion delegates, eliminating the need for developers to manually provide assertion callbacks.
/// </para>
/// <para>
/// <strong>Design Pattern: Framework Adapter</strong>
/// </para>
/// <para>
/// This class follows the adapter pattern by:
/// </para>
/// <list type="bullet">
///   <item>
///     <strong>Inheriting:</strong> Extends <see cref="Portamical.Assertions.PortamicalAssert"/> to access
///     framework-agnostic assertion logic.
///   </item>
///   <item>
///     <strong>Simplifying:</strong> Provides methods with fewer parameters by pre-configuring TUnit-specific
///     assertion delegates (e.g., <c>Assert.EqualityFail</c>, <c>Assert.That</c>).
///   </item>
///   <item>
///     <strong>Bridging:</strong> Translates between TUnit's assertion model and the base class's delegate-based
///     approach.
///   </item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> All members are static and thread-safe (stateless design).
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage:</strong></para>
/// <code>
/// using Portamical.TUnit.Assertions;
/// 
/// [Test]
/// public async Task DoesNotThrow_ValidOperation_NoException()
/// {
///     // Simplified API - no need to pass Assert.EqualityFail
///     PortamicalAssert.DoesNotThrow(() => myService.DoWork());
/// }
/// 
/// [Test]
/// public async Task ThrowsDetails_InvalidOperation_MatchesExpected()
/// {
///     var expected = new InvalidOperationException("Cannot process");
///     
///     // Validates exception type and message automatically
///     var actual = PortamicalAssert.ThrowsDetails(
///         () => myService.ProcessInvalid(),
///         expected);
///     
///     Assert.That(actual.Message == expected.Message);
/// }
/// </code>
/// 
/// <para><strong>ArgumentException with ParamName:</strong></para>
/// <code>
/// [Test]
/// public async Task ThrowsDetails_ArgumentException_ValidatesParamName()
/// {
///     var expected = new ArgumentNullException("userId", "User ID cannot be null");
///     
///     var actual = PortamicalAssert.ThrowsDetails(
///         () => myService.GetUser(null!),
///         expected);
///     
///     // ParamName is automatically validated when non-null in expected
///     Assert.That(actual.ParamName == "userId");
/// }
/// </code>
/// 
/// <para><strong>Selective Metadata Assertion:</strong></para>
/// <code>
/// [Test]
/// public async Task ThrowsDetails_OnlyTypeMatters_SkipMessage()
/// {
///     // Pass null message to skip message assertion
///     var expected = new InvalidOperationException(message: null);
///     
///     // Only validates exception type, not message
///     PortamicalAssert.ThrowsDetails(
///         () => myService.ThrowSomething(),
///         expected);
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.Assertions.PortamicalAssert"/>
/// <seealso cref="DoesNotThrow(Action)"/>
/// <seealso cref="ThrowsDetails{TException}(Action, TException)"/>
public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    #region Assert Methods
    /// <summary>
    /// Verifies that the specified action does not throw an exception using TUnit's assertion framework.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's <see cref="Portamical.Assertions.PortamicalAssert.DoesNotThrow"/>
    /// method and automatically provides TUnit's assertion failure handling.
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>Verifying that operations complete without throwing exceptions</item>
    ///   <item>Testing "happy path" scenarios where no errors are expected</item>
    ///   <item>Validating guard clause logic that should allow valid inputs</item>
    /// </list>
    /// </remarks>
    /// <param name="attempt">The action to execute and verify for the absence of exceptions. Cannot be null.</param>
    /// <exception cref="AssertionException">Thrown by TUnit when the action throws an exception.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task Calculate_ValidInput_NoException()
    /// {
    ///     PortamicalAssert.DoesNotThrow(() => calculator.Add(2, 3));
    /// }
    /// 
    /// [Test]
    /// public async Task Initialize_NormalConditions_NoException()
    /// {
    ///     var service = new MyService();
    ///     PortamicalAssert.DoesNotThrow(() => service.Initialize());
    /// }
    /// </code>
    /// </example>
    public static void DoesNotThrow(Action attempt)
    => DoesNotThrow(attempt, Fail);

    /// <summary>
    /// Executes the specified action and verifies that it throws an exception of the expected type and with matching
    /// details using TUnit's assertion framework.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's <see cref="Portamical.Assertions.PortamicalAssert.ThrowsDetails{TException}"/>
    /// method and automatically provides TUnit-specific assertion delegates for:
    /// </para>
    /// <list type="bullet">
    ///   <item><strong>Exception Capture:</strong> <see cref="CatchException(Action)"/></item>
    ///   <item><strong>Type Validation:</strong> TUnit's type checking via <c>Assert.That</c></item>
    ///   <item><strong>Equality Assertions:</strong> TUnit's <c>Assert.That</c> for string comparisons</item>
    ///   <item><strong>Failure Handling:</strong> TUnit's <c>Assert.EqualityFail</c></item>
    /// </list>
    /// <para>
    /// <strong>Selective Assertion Pattern:</strong> Properties are only asserted if set (non-null) in the
    /// expected exception. Use null values to skip assertions for properties that are implementation details.
    /// See <see cref="Portamical.Assertions.PortamicalAssert.ThrowsMetadataEquality{TException}"/> for details.
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>Testing guard clauses and input validation</item>
    ///   <item>Verifying error handling behavior</item>
    ///   <item>Asserting specific error messages or parameter names</item>
    ///   <item>Testing exception-based control flow</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TException">The type of exception expected to be thrown by the action. Must be a non-null reference type derived from
    /// Exception.</typeparam>
    /// <param name="attempt">The action to execute, which is expected to throw an exception of type TException. Cannot be null.</param>
    /// <param name="expected">The expected exception instance, used as a reference for type and detail comparisons.
    /// Set <c>Message</c> or <c>ParamName</c> (for ArgumentException) to null to skip those assertions.</param>
    /// <returns>The actual exception of type TException that was thrown by the action and verified to match the expected
    /// details.</returns>
    /// <exception cref="AssertionException">Thrown by TUnit when:
    /// <list type="bullet">
    ///   <item>No exception is thrown when one was expected</item>
    ///   <item>The thrown exception type doesn't match the expected type</item>
    ///   <item>The exception message doesn't match (when expected.Message is non-null)</item>
    ///   <item>The ParamName doesn't match (when expected is ArgumentException with non-null ParamName)</item>
    /// </list>
    /// </exception>
    /// <example>
    /// <para><strong>Full Validation (Type + Message + ParamName):</strong></para>
    /// <code>
    /// [Test]
    /// public async Task GetUser_NullId_ThrowsArgumentNull()
    /// {
    ///     var expected = new ArgumentNullException("userId", "User ID cannot be null");
    ///     
    ///     var actual = PortamicalAssert.ThrowsDetails(
    ///         () => userService.GetUser(null!),
    ///         expected);
    ///     
    ///     // All properties validated: Type, Message, ParamName
    ///     Assert.That(actual.ParamName == "userId");
    /// }
    /// </code>
    /// 
    /// <para><strong>Type-Only Validation (Skip Message):</strong></para>
    /// <code>
    /// [Test]
    /// public async Task Process_InvalidState_ThrowsInvalidOperation()
    /// {
    ///     // Set message to null to skip message assertion
    ///     var expected = new InvalidOperationException(message: null);
    ///     
    ///     var actual = PortamicalAssert.ThrowsDetails(
    ///         () => processor.ProcessInvalid(),
    ///         expected);
    ///     
    ///     // Only exception type is validated
    ///     Assert.That(actual != null);
    /// }
    /// </code>
    /// 
    /// <para><strong>ArgumentException Without ParamName Validation:</strong></para>
    /// <code>
    /// [Test]
    /// public async Task Validate_EmptyString_ThrowsArgument()
    /// {
    ///     // ParamName is null, so it won't be asserted
    ///     var expected = new ArgumentException("Value cannot be empty", paramName: null);
    ///     
    ///     var actual = PortamicalAssert.ThrowsDetails(
    ///         () => validator.Validate(""),
    ///         expected);
    ///     
    ///     // Only type and message validated, ParamName skipped
    /// }
    /// </code>
    /// </example>
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(attempt, expected,
        catchException: CatchException,
        assertIsType: IsTypeOf,
        assertEquality: Equality,
        assertFail: Fail);

    public static void Fail(string? message)
    => throw new AssertionException(message);

    public static void IsTypeOf(Type expectedType, object? actual)
    => IsTypeOf(expectedType, actual,
        assertEquality: Equality);

    static void Equality(Type expected, Type? actual)
    => Equality(expected, actual,
        equals: (e, a) => actual == expected,
        assertFail: Fail,
        message: GetNotExpectedTypeExceptionThrownMessage(expected, actual));

    public static void Equality(object expected, object? actual)
    => Equality(expected, actual,
        assertFail: () => Fail(
            $"Expected '{expected}' but '{actual ?? "null"}' returned."));
    #endregion
}
