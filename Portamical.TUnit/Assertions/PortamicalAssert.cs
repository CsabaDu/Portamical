// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using TUnit.Assertions.Exceptions;

namespace Portamical.TUnit.Assertions;

/// <summary>
/// Provides TUnit-specific assertion helper methods extending the framework-agnostic async-first base class.
/// </summary>
/// <remarks>
/// <para>
/// This class adapts the async-first assertion logic from <see cref="Portamical.Assertions.PortamicalAssert"/>
/// (version 2.2.0+) to work seamlessly with TUnit's async assertion framework. It provides simplified APIs
/// that automatically wire up TUnit-specific assertion delegates.
/// </para>
/// <para>
/// <strong>Design Pattern: Framework Adapter for Async-First Testing</strong>
/// </para>
/// <para>
/// This class follows the adapter pattern optimized for TUnit's async-first philosophy by:
/// </para>
/// <list type="bullet">
///   <item>
///     <strong>Inheriting:</strong> Extends <see cref="Portamical.Assertions.PortamicalAssert"/> to access
///     framework-agnostic async assertion logic.
///   </item>
///   <item>
///     <strong>Simplifying:</strong> Provides methods with fewer parameters by pre-configuring TUnit-specific
///     assertion delegates that throw <see cref="AssertionException"/>.
///   </item>
///   <item>
///     <strong>Zero Allocation:</strong> Uses <see cref="ValueTask"/> to minimize allocations for
///     synchronous completion paths (successful assertions).
///   </item>
///   <item>
///     <strong>Async Native:</strong> All methods return <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>
///     following TUnit's async-by-default design.
///   </item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> All members are static and thread-safe (stateless design).
/// </para>
/// <para>
/// <strong>Versioning:</strong> Requires Portamical (shared) version 2.2.0 or later for async-first support.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage (Async-First):</strong></para>
/// <code>
/// using Portamical.TUnit.Assertions;
/// 
/// [Test]
/// public async Task DoesNotThrow_ValidOperation_NoException()
/// {
///     // Async assertion - returns ValueTask
///     await PortamicalAssert.DoesNotThrow(() => myService.DoWork());
/// }
/// 
/// [Test]
/// public async Task ThrowsDetails_InvalidOperation_MatchesExpected()
/// {
///     var expected = new InvalidOperationException("Cannot process");
///     
///     // Validates exception type and message, returns ValueTask&lt;TException&gt;
///     var actual = await PortamicalAssert.ThrowsDetails(
///         () => myService.ProcessInvalid(),
///         expected);
///     
///     await Assert.That(actual.Message).IsEqualTo(expected.Message);
/// }
/// </code>
/// 
/// <para><strong>ArgumentException with ParamName Validation:</strong></para>
/// <code>
/// [Test]
/// public async Task ThrowsDetails_ArgumentException_ValidatesParamName()
/// {
///     var expected = new ArgumentNullException("userId", "User ID cannot be null");
///     
///     // ParamName is automatically validated when non-null in expected
///     var actual = await PortamicalAssert.ThrowsDetails(
///         () => myService.GetUser(null!),
///         expected);
///     
///     await Assert.That(actual.ParamName).IsEqualTo("userId");
/// }
/// </code>
/// 
/// <para><strong>Selective Metadata Assertion (Type-Only):</strong></para>
/// <code>
/// [Test]
/// public async Task ThrowsDetails_OnlyTypeMatters_SkipMessage()
/// {
///     // Pass null message to skip message assertion
///     var expected = new InvalidOperationException(message: null);
///     
///     // Only validates exception type, not message
///     var actual = await PortamicalAssert.ThrowsDetails(
///         () => myService.ThrowSomething(),
///         expected);
///     
///     await Assert.That(actual).IsNotNull();
/// }
/// </code>
/// 
/// <para><strong>Zero Allocation Success Path:</strong></para>
/// <code>
/// [Test]
/// public async Task DoesNotThrow_ZeroAllocation()
/// {
///     // When no exception is thrown, returns default(ValueTask) - zero allocation
///     await PortamicalAssert.DoesNotThrow(() => calculator.Add(2, 3));
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.Assertions.PortamicalAssert"/>
/// <seealso cref="Fail(string)"/>
/// <seealso cref="Fail()"/>
/// <seealso cref="DoesNotThrow(Action)"/>
/// <seealso cref="ThrowsDetails{TException}(Action, TException)"/>
/// <seealso cref="Equality{T}(T, T, Func{T, T, bool}, string)"/>
/// <seealso cref="Equality(object, object, double?)"/>
/// <seealso cref="IsTypeOf(Type, object)"/>
public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    #region Assert Methods

    /// <summary>
    /// Fails the assertion with the specified message (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the primary async-first implementation that throws <see cref="AssertionException"/>
    /// to signal test failure in TUnit.
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>Explicitly failing a test when a condition is not met</item>
    ///   <item>Custom assertion logic that requires manual failure</item>
    ///   <item>Unconditional test failure for testing framework behavior</item>
    /// </list>
    /// </remarks>
    /// <param name="message">The failure message to include in the exception. Can be null.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that never completes normally (always throws).
    /// </returns>
    /// <exception cref="AssertionException">Always thrown to signal the assertion failure.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task CustomValidation_InvalidState_Fails()
    /// {
    ///     if (someComplexCondition)
    ///     {
    ///         await PortamicalAssert.Fail("Complex condition not met");
    ///     }
    /// }
    /// 
    /// [Test]
    /// public async Task ShouldNotReachHere_ExecutionPath_Fails()
    /// {
    ///     if (result == null)
    ///     {
    ///         await PortamicalAssert.Fail("Result should never be null");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static ValueTask Fail(string? message)
    => throw new AssertionException(message);

    /// <summary>
    /// Fails the assertion without a message (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Convenience overload for failing assertions when no specific message is needed.
    /// </para>
    /// </remarks>
    /// <returns>
    /// A <see cref="ValueTask"/> that never completes normally (always throws).
    /// </returns>
    /// <exception cref="AssertionException">Always thrown to signal the assertion failure.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task UnreachableCode_ShouldNeverExecute()
    /// {
    ///     await PortamicalAssert.Fail();
    /// }
    /// </code>
    /// </example>
    public static ValueTask Fail()
    => Fail(null);

    /// <summary>
    /// Verifies that the specified action does not throw an exception using TUnit's assertion framework.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's async-first
    /// <see cref="Portamical.Assertions.PortamicalAssert.DoesNotThrowAsync(Action, Func{string, ValueTask})"/>
    /// method and automatically provides TUnit's assertion failure handling.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Returns <c>default(ValueTask)</c> when no exception is thrown,
    /// resulting in zero allocation for successful assertions.
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
    /// <returns>
    /// A <see cref="ValueTask"/> that completes when the assertion finishes.
    /// Returns <c>default(ValueTask)</c> (zero allocation) on success.
    /// </returns>
    /// <exception cref="AssertionException">Thrown when the action throws an exception.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task Calculate_ValidInput_NoException()
    /// {
    ///     await PortamicalAssert.DoesNotThrow(() => calculator.Add(2, 3));
    /// }
    /// 
    /// [Test]
    /// public async Task Initialize_NormalConditions_NoException()
    /// {
    ///     var service = new MyService();
    ///     await PortamicalAssert.DoesNotThrow(() => service.Initialize());
    /// }
    /// 
    /// [Test]
    /// public async Task ProcessData_ValidInput_Completes()
    /// {
    ///     // Zero allocation when successful
    ///     await PortamicalAssert.DoesNotThrow(() => 
    ///     {
    ///         processor.Process(validData);
    ///         processor.Save();
    ///     });
    /// }
    /// </code>
    /// </example>
    public static ValueTask DoesNotThrow(Action attempt)
    => DoesNotThrowAsync(
        attempt,
        assertFailAsync: Fail);

    /// <summary>
    /// Executes the specified action and verifies that it throws an exception of the expected type
    /// with matching details using TUnit's assertion framework.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's async-first
    /// <see cref="Portamical.Assertions.PortamicalAssert.ThrowsDetailsAsync{TException}"/>
    /// method and automatically provides TUnit-specific assertion delegates.
    /// </para>
    /// <para>
    /// <strong>Selective Assertion Pattern:</strong> Properties are only asserted if set (non-null) in the
    /// expected exception. Use null values to skip assertions for properties that are implementation details:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <strong>Message:</strong> Set to non-null to assert message equality. Set to null to skip.
    ///   </item>
    ///   <item>
    ///     <strong>ParamName (ArgumentException):</strong> Set to non-null to assert parameter name equality.
    ///     Set to null to skip.
    ///   </item>
    /// </list>
    /// <para>
    /// See <see cref="Portamical.Assertions.PortamicalAssert"/> base class documentation for details
    /// on the selective assertion pattern.
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
    /// <typeparam name="TException">
    /// The type of exception expected to be thrown by the action. Must be a non-null reference type
    /// derived from Exception.
    /// </typeparam>
    /// <param name="attempt">
    /// The action to execute, which is expected to throw an exception of type TException. Cannot be null.
    /// </param>
    /// <param name="expected">
    /// The expected exception instance, used as a reference for type and detail comparisons.
    /// Set <c>Message</c> or <c>ParamName</c> (for ArgumentException) to null to skip those assertions.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the actual exception of type TException that was
    /// thrown by the action and verified to match the expected details.
    /// </returns>
    /// <exception cref="AssertionException">
    /// Thrown when:
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
    ///     var actual = await PortamicalAssert.ThrowsDetails(
    ///         () => userService.GetUser(null!),
    ///         expected);
    ///     
    ///     // All properties validated: Type, Message, ParamName
    ///     await Assert.That(actual.ParamName).IsEqualTo("userId");
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
    ///     var actual = await PortamicalAssert.ThrowsDetails(
    ///         () => processor.ProcessInvalid(),
    ///         expected);
    ///     
    ///     // Only exception type is validated
    ///     await Assert.That(actual).IsNotNull();
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
    ///     var actual = await PortamicalAssert.ThrowsDetails(
    ///         () => validator.Validate(""),
    ///         expected);
    ///     
    ///     // Only type and message validated, ParamName skipped
    ///     await Assert.That(actual.Message).Contains("cannot be empty");
    /// }
    /// </code>
    /// 
    /// <para><strong>Multiple Exception Properties:</strong></para>
    /// <code>
    /// [Test]
    /// public async Task ParseValue_InvalidFormat_ThrowsFormatException()
    /// {
    ///     var expected = new FormatException("Invalid number format: 'abc'");
    ///     
    ///     var actual = await PortamicalAssert.ThrowsDetails(
    ///         () => parser.Parse("abc"),
    ///         expected);
    ///     
    ///     await Assert.That(actual.Message).IsEqualTo(expected.Message);
    /// }
    /// </code>
    /// </example>
    public static ValueTask<TException> ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    => ThrowsDetailsAsync(
        attempt,
        expected,
        catchException: CatchException,
        assertIsTypeAsync: (expectedType, actual) =>
        {
            var actualType = actual.GetType();

            if (actualType != expectedType)
            {
                return Fail(GetNotExpectedTypeExceptionThrownMessage(expectedType, actualType));
            }

            return default;  // Zero allocation success
        },
        assertEqualityAsync: (expectedValue, actualValue) =>
        {
            if (!Equals(expectedValue, actualValue))
            {
                return Fail(GetNotExpectedValueMessage(
                    expectedValue ?? "null",
                    actualValue));
            }

            return default;  // Zero allocation success
        },
        assertFailAsync: Fail);

    /// <summary>
    /// Verifies value equality using a custom equality comparer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's async-first
    /// <see cref="Portamical.Assertions.PortamicalAssert.EqualityAsync{T}"/>
    /// method and provides TUnit-specific assertion failure handling.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Returns <c>default(ValueTask)</c> when values are equal,
    /// resulting in zero allocation for successful assertions.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of values being compared.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="equals">A delegate that determines whether two values are equal.</param>
    /// <param name="message">The failure message to use if values are not equal.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that completes when the assertion finishes.
    /// Returns <c>default(ValueTask)</c> (zero allocation) on success.
    /// </returns>
    /// <exception cref="AssertionException">Thrown when the values are not equal according to the comparer.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task CustomEquality_ComparesCorrectly()
    /// {
    ///     var expected = new Person("John", 30);
    ///     var actual = new Person("John", 30);
    ///     
    ///     await PortamicalAssert.Equality(
    ///         expected,
    ///         actual,
    ///         (e, a) => e?.Name == a?.Name && e?.Age == a?.Age,
    ///         "Persons should be equal by name and age");
    /// }
    /// </code>
    /// </example>
    public static ValueTask Equality<T>(
        T? expected,
        T? actual,
        Func<T?, T?, bool> equals,
        string message)
    => EqualityAsync(
        expected,
        actual,
        equals,
        assertFailAsync: Fail,
        message);

    /// <summary>
    /// Verifies value equality for common primitive and framework types with automatic comparison.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's async-first
    /// <see cref="Portamical.Assertions.PortamicalAssert.EqualityAsync(object, object, Func{ValueTask}, double?)"/>
    /// method and provides TUnit-specific assertion failure handling.
    /// </para>
    /// <para>
    /// <strong>Supported Types:</strong> Primitives, strings, DateTime, TimeSpan, Guid, enums,
    /// floating-point types (with tolerance), and collections (recursive comparison).
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Returns <c>default(ValueTask)</c> when values are equal,
    /// resulting in zero allocation for successful assertions.
    /// </para>
    /// </remarks>
    /// <param name="expected">The expected value. Cannot be null.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="floatingPointTolerance">
    /// Epsilon for floating-point comparisons. Default: 1e-10 for double, 1e-6f for float.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> that completes when the assertion finishes.
    /// Returns <c>default(ValueTask)</c> (zero allocation) on success.
    /// </returns>
    /// <exception cref="AssertionException">Thrown when the values are not equal.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task Equality_IntegersMatch()
    /// {
    ///     await PortamicalAssert.Equality(expected: 42, actual: calculator.Add(40, 2));
    /// }
    /// 
    /// [Test]
    /// public async Task Equality_FloatingPointWithTolerance()
    /// {
    ///     await PortamicalAssert.Equality(
    ///         expected: 3.14159,
    ///         actual: Math.PI,
    ///         floatingPointTolerance: 0.00001);
    /// }
    /// 
    /// [Test]
    /// public async Task Equality_Strings()
    /// {
    ///     await PortamicalAssert.Equality(
    ///         expected: "Hello, World!",
    ///         actual: service.GetGreeting());
    /// }
    /// </code>
    /// </example>
    public static ValueTask Equality(
        object expected,
        object? actual,
        double? floatingPointTolerance = null)
    => EqualityAsync(
        expected,
        actual,
        assertFailAsync: () => Fail(GetNotExpectedValueMessage(expected, actual)),
        floatingPointTolerance);

    /// <summary>
    /// Verifies that the runtime type of an object matches the expected type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method wraps the base class's async-first
    /// <see cref="Portamical.Assertions.PortamicalAssert.IsTypeOfAsync"/>
    /// method and provides TUnit-specific assertion failure handling.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Minimal allocation when types match.
    /// </para>
    /// </remarks>
    /// <param name="expected">The expected type. Cannot be null.</param>
    /// <param name="actual">The object whose runtime type is to be verified.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when the assertion finishes.</returns>
    /// <exception cref="AssertionException">Thrown when the actual object's type doesn't match the expected type.</exception>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task Create_ReturnsCorrectType()
    /// {
    ///     var result = factory.Create();
    ///     await PortamicalAssert.IsTypeOf(typeof(ConcreteClass), result);
    /// }
    /// </code>
    /// </example>
    public static ValueTask IsTypeOf(Type expected, object? actual)
    => IsTypeOfAsync(
        expected,
        actual,
        assertEqualityAsync: (expectedType, actualType) =>
        {
            if (expectedType != actualType)
            {
                return Fail(GetNotExpectedTypeExceptionThrownMessage(expectedType, actualType));
            }

            return default;  // Zero allocation success
        });

    #endregion
}
